from music21 import *
import os
import numpy as np
from tensorflow.keras.utils import to_categorical


def generateAndReplaceInput(model, VOCAB_SIZE, intToNote, noteToInt, toGenerate=500, inputSongTokens=[], generateUntilEnd=False):
    generatedTokens = generateSubsequentTokens(model, VOCAB_SIZE, intToNote, toGenerate=toGenerate, startingInput=inputSongTokens, generateUntilEnd=False)

    print("Replacing input for next prediction")
    inputSongTokens = [noteToInt[i] for i in generatedTokens[-50:]]

    print("Converting text to midi")
    genmidistream = text2midi(generatedTokens)
    print("Converting midi to Unity tokens")
    genmidistream = midi2textSeconds(genmidistream)

    return inputSongTokens, genmidistream

def generateSubsequentTokens(model, VOCAB_SIZE, intToNote, toGenerate=500, startingInput=[], generateUntilEnd=False):
    # Range determines the number of tokens to predict

    keepGenerating = True
    tokensGenerated = 0

    slidingSequence = [startingInput.copy()]
    predictionOutput = []

    while keepGenerating:
        # Convert to acceptable format for trained model
        prediction_input = to_categorical(slidingSequence, num_classes=VOCAB_SIZE + 1)

        # Predict next token depending on the previous 50 tokens
        prediction = model(prediction_input)
        index = np.argmax(prediction)

        # Check if previous tokens were "varied" enough, if not, choose a random prediction from the top 3 predictions
        if (len(np.unique(slidingSequence)) < 15):
           ind = np.argpartition(prediction[0], -2)[-2:]
           index = np.random.choice(ind)

        result = intToNote[index]

        predictionOutput.append(result)
        tokensGenerated += 1

        slidingSequence = np.append(slidingSequence, index)
        slidingSequence = [slidingSequence[1:len(slidingSequence)]]

        if (generateUntilEnd and result == "end") or (not generateUntilEnd and tokensGenerated == toGenerate):
            keepGenerating = False

    return predictionOutput

def open_midi(midi_path):
    mf = converter.parse(midi_path)
    return mf


# Restricts possible velocities to 8 values, keeping the number of unique note events smaller
# Resembles ppp, pp, p, mp, mf, f, ff, fff dynamics
def vModifier(velocity):
    if (velocity == 0):
        return 0

    velocity = min(127, ((velocity // 16) + 1) * 16)
    return velocity


def tModifier(tempo):
    if (tempo == 0):
        return 0

    tempo = ((tempo // 10) + 1) * 10
    return tempo


# Check if there are notes which should have ended before given offset
def checkForNoteOffEvent(currentOffset, noteOffEvents):
    notesToEnd = []

    for noteOffEvent in noteOffEvents:  # for (notename, endingOffset)
        if noteOffEvent[1] <= currentOffset:
            notesToEnd.append(noteOffEvent)

    return notesToEnd


# Access midifile with Parts merged together with correct offsets

def midi2text(midifile):
    previousElementOffset = 0.0
    offsetChanged = False

    tempoRetrieved = False
    timeSigRetrieved = False

    currentVelocity = 0

    tokens = []
    noteOffEvents = []

    tokens.append("START")

    for element in midifile.flat.elements:
        # print(type(element))

        currentElementOffset = element.offset

        notesToEnd = checkForNoteOffEvent(currentElementOffset, noteOffEvents)

        if (len(notesToEnd) != 0):
            for noteToEnd in notesToEnd:
                difference = float(noteToEnd[1]) - float(previousElementOffset)
                if (difference > 0.01):
                    tokens.append("wait:" + str(round(difference, 5)))
                    previousElementOffset = noteToEnd[1]
                tokens.append("note:" + str(noteToEnd[0]) + ":OFF")
                noteOffEvents.remove(noteToEnd)

        # If offset has increased and we're looking at new notes, add a wait event before adding the new notes
        if (float(currentElementOffset) > float(previousElementOffset + 0.01) and (
                isinstance(element, note.Note) or isinstance(element, chord.Chord))):
            offsetChanged = True
            difference = float(currentElementOffset - previousElementOffset)
            tokens.append("wait:" + str(round(difference, 5)))

        if (isinstance(element, tempo.MetronomeMark) and not tempoRetrieved):
            tempoRetrieved = True
            tokens.append("tempo:" + str(tModifier(element.number)))

        if (isinstance(element, meter.TimeSignature) and not timeSigRetrieved):
            timeSigRetrieved = True
            tokens.append("timesig:" + str(element.ratioString))

        if (isinstance(element, note.Note)):  # This is a note event, add a token for this note
            if (currentVelocity != vModifier(element.volume.velocity)):
                currentVelocity = vModifier(element.volume.velocity)
                tokens.append("velocity:" + str(currentVelocity))
            tokens.append("note:" + str(element.pitch))
            noteOffEvents.append((str(element.pitch), float(currentElementOffset + element.duration.quarterLength), 5))

        if (isinstance(element, chord.Chord)):  # This is a chord event, add a token for each note in chord
            for chordnote in element:
                if (currentVelocity != vModifier(element.volume.velocity)):
                    currentVelocity = vModifier(element.volume.velocity)
                    tokens.append("velocity:" + str(currentVelocity))
                tokens.append("note:" + str(chordnote.pitch))
                noteOffEvents.append(
                    (str(chordnote.pitch), float(currentElementOffset + element.duration.quarterLength)))

        if (offsetChanged):
            previousElementOffset = currentElementOffset
            offsetChanged = False

    # Finally make sure that all notes that end after the offset of the last element of mf.flat.elements are given an off event.
    for noteToEnd in noteOffEvents.copy():
        difference = float(noteToEnd[1]) - float(previousElementOffset)
        if (difference > 0.01):
            tokens.append("wait:" + str(round(difference, 5)))
            previousElementOffset = noteToEnd[1]
        tokens.append("note:" + str(noteToEnd[0]) + ":OFF")
        noteOffEvents.remove(noteToEnd)

    if (len(noteOffEvents) != 0):
        print("Not all notes have note-off events")

    tokens.append("END")
    return [token.lower() for token in tokens]


def midi2textSeconds(midifile):
    # If loaded from file, grab stream
    # Since this method is solely for generated midifiles, then there will always be only one stream
    # if isinstance(midifile, stream.Score):
    #    midifile = midifile[0]

    tokens = []

    for element in midifile.flat.secondsMap:

        if isinstance(element['element'], note.Note):
            noteName = element['element'].pitch.midi
            offsetSeconds = round(element['offsetSeconds'], 4)
            durationSeconds = round(element['durationSeconds'], 4)
            velocity = element['element'].volume.velocity
            tokens.append("{0}:{1}:{2}:{3}".format(noteName, offsetSeconds, durationSeconds, velocity))
            # print("{0}:{1}:{2}:{3}".format(noteName, offsetSeconds, durationSeconds, velocity))

        if isinstance(element['element'], chord.Chord):
            for chordNote in element['element']:
                noteName = chordNote.pitch.midi
                offsetSeconds = round(element['offsetSeconds'], 4)
                durationSeconds = round(element['durationSeconds'], 4)
                velocity = element['element'].volume.velocity
                tokens.append("{0}:{1}:{2}:{3}".format(noteName, offsetSeconds, durationSeconds, velocity))
                # print("{0}:{1}:{2}:{3}".format(noteName, offsetSeconds, durationSeconds, velocity))

    return tokens


def text2midi(tokens):
    s = stream.Stream()

    currentVelocity = 80

    currentOffset = 0
    currentToken = 0

    for token in tokens:

        splitToken = token.split(":")

        if token.startswith("tempo"):
            s.append(tempo.MetronomeMark(number=float(splitToken[1])))

        if token.startswith("timesig"):
            s.append(meter.TimeSignature(splitToken[1]))

        if token.startswith("velocity"):
            currentVelocity = int(splitToken[1])

        if token.startswith("note") and not token.lower().endswith("off"):
            noteDuration = 0
            noteName = splitToken[1]

            for element in tokens[currentToken + 1:]:
                splitToken2 = element.split(":")
                if (element.startswith("wait")):
                    noteDuration += float(splitToken2[1])
                if (element.startswith("note") and element.lower().endswith("off")):
                    if (noteName == splitToken2[1]):
                        newNote = note.Note(nameWithOctave=splitToken[1],
                                            quarterLength=round(float(noteDuration), 5))
                        newNote.volume.velocity = currentVelocity
                        s.insert(currentOffset, newNote)
                        break

        if token.startswith("wait"):
            currentOffset += float(splitToken[1])

        currentToken += 1

    return s

