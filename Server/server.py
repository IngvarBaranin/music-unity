# Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

# Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
# It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

# Use at your own risk
# Use under the Apache License 2.0

# Example of a Python UDP server

from pred_utils import *
import UdpComms as U
import time

import tensorflow as tf
import tensorflow.keras

from tensorflow.keras.preprocessing import sequence
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Embedding, LSTM, Bidirectional, Input, GlobalMaxPool1D, Activation
from tensorflow.keras.callbacks import ModelCheckpoint

import json
import numpy as np
import os

#print("Num GPUs Available: ", len(tf.config.experimental.list_physical_devices('GPU')))


#try:
intToNote = {int(k):(v) for k,v in json.load(open("./dictionary/dictionary.json")).items()}
noteToInt = dict(map(reversed, intToNote.items()))

VOCAB_SIZE = len(intToNote)
print("Instantiating model")
model = tf.keras.models.load_model("./trained_models/LSTM256-generator2.h5")

print("Listening")
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)




print("Creating starting input")
inputSongTokens = [noteToInt[i] for i in midi2text(open_midi("./testmidis/supermario.mid"))][:50]
readyToSend = False

print("First prediction")
inputSongTokens, GenMidiStream = generateAndReplaceInput(model, VOCAB_SIZE, intToNote, noteToInt, toGenerate=200, inputSongTokens=inputSongTokens)


while True:
    if readyToSend:
        print("Sending", len(GenMidiStream), "notes to Unity")
        sock.SendData(" ".join(GenMidiStream))

        print("Pre-predicting")
        inputSongTokens, GenMidiStream = generateAndReplaceInput(model, VOCAB_SIZE, intToNote, noteToInt, toGenerate=200, inputSongTokens=inputSongTokens)

        readyToSend = False

    data = sock.ReadReceivedData() # read data

    if data != None: # if NEW data has been received since last ReadReceivedData function call
        if (data == "True"):
            readyToSend = True

    time.sleep(0.1)

#except Exception as ex:
#    print("Server crash", ex)
#    input()