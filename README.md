![alt text](https://i.imgur.com/WPimOXJ.png)

# Infinite Runner with Adaptive Background Music Generated in Real Time

A simple 2D side-scroller with the objective of getting as far as possible in a world of procedurally generated platforms, with the twist that all of the background music is generated in real time, while also adapting to the movements of the player.

## How?

At the start of the game, Unity starts a Python server, which loads up a TensorFlow-trained LSTM model. The model starts generating musical events which are then sent to Unity through an UDP connection (thanks to [Y. T. Elashry's Two-way communication between Python 3 and Unity](https://github.com/Siliconifier/Python-Unity-Socket-Communication)). Once the musical events - tokens - reach Unity, [Maestro MPTK](https://assetstore.unity.com/packages/tools/audio/maestro-midi-player-tool-kit-free-107994) is used to play the tokens.

To make sure that the music is played in real time, the TensorFlow model sends tokens in small chunks. Under the hood, it is a race between the speed of TensorFlow's generating and how fast each musical chunk is played, so having TensorFlow perform predictions on a GPU is vital for the music to work properly.

For adaptivity, Unity communicates the player's y-coordinate location in the game world back to Python, which the model takes as input to see whether it should prefer higher or lower musical pitches. Other musical parameters, such as tempo and instrument, which are tied to player speed and platform type, are changed within Unity. 

Setting this project up yourself may be tedious, so for a quick demo video, see [here.](https://youtu.be/W4V6h4S4reI)

This was, without a doubt, a very fun project, however in the future I hope to create a less performance-hungry solution. 

## Training the generative model

The training part of this project can be found [here.](https://github.com/IngvarBaranin/ai_music_generation)
