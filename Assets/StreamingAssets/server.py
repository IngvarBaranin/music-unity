# Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

# Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
# It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

# Use at your own risk
# Use under the Apache License 2.0

from pred_utils import *
import UdpComms as U
import time

import tensorflow as tf
import tensorflow.keras

from tensorflow.compat.v1.keras.backend import set_session
from tensorflow.keras.utils import to_categorical

import json
import numpy as np
import os

#print("Num GPUs Available: ", len(tf.config.experimental.list_physical_devices('GPU')))


#os.environ["CUDA_VISIBLE_DEVICES"] = "-1"

#try:
config = tf.compat.v1.ConfigProto()
config.gpu_options.allow_growth = True  # dynamically grow the memory used on the GPU
config.log_device_placement = True  # to log device placement (on which device the operation ran)
sess = tf.compat.v1.Session(config=config)
set_session(sess)  # set this TensorFlow session as the default session for Keras

intToNote = {int(k):(v) for k,v in json.load(open("./dictionary/dictionary.json")).items()}
noteToInt = dict(map(reversed, intToNote.items()))

VOCAB_SIZE = len(intToNote)
print("Instantiating model")
model = tf.keras.models.load_model("./trained_models/LSTM256-generator2.h5")

print("Listening")
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)




print("Creating starting input")
inputSongTokens = np.random.randint(1, 300, size=50).tolist()
readyToSend = False
height = None

print("First prediction")
inputSongTokens, GenMidiStream = generateAndReplaceInput(model, VOCAB_SIZE, intToNote, noteToInt, toGenerate=50, inputSongTokens=inputSongTokens)


while True:
    if readyToSend:
        print("Sending", len(GenMidiStream), "notes to Unity")
        sock.SendData(" ".join(GenMidiStream))

        #print("Pre-predicting")
        inputSongTokens, GenMidiStream = generateAndReplaceInput(model, VOCAB_SIZE, intToNote, noteToInt, toGenerate=50, inputSongTokens=inputSongTokens, height=height)

        readyToSend = False

    data = sock.ReadReceivedData() # read data

    if data != None: # if NEW data has been received since last ReadReceivedData function call
        dataSplit = data.split(":", 1)
        print(dataSplit)
        if (dataSplit[0] == "True"):
            readyToSend = True
            height = int(dataSplit[1])
            print(height)
        if (dataSplit[0] == "Quit"):
            quit()

    time.sleep(0.1)
#except Exception as e:
#    print(e)
#    #input()