# Autisim eye gazes recognizer
The purpose of the program is to analyze thoughts and feelings of a person by his eye gazes. 
The project tests the person’s reaction in a VR environment that visualizes a room with 2 posters. Each poster is a possible answer for a cognitive or a feeling test . The person should look at the posters and by recording his eye gazes positions we will try to determine if he knows the correct answer (or have the expected feeling). The data will be sent to Firebase database and with the ionic app we will be able to watch the customize charts of the regarding dashboard. What does the project is comprised of? The project includes the Unity Project (which handles the FOVE device ) and the ionic project and of course the Firebase project that is responsible for two main roles:

- storing the app data
- hosting and maintaining the app

*this project is based on Fove sample , the files that I wrote/edited might be found at the folders : Objects, LogsMethods, Scene and GameProcess. later at this article I will describe a few of them.*

![alt text](https://raw.githubusercontent.com/amitme10/EyesGazeRecognitionFove/master/expl.png)
# Examples from the dashboard

So as I mentioned before the main purpose of the project is to analyze the data , so I created a dashboard that helps with it , a few charts of examples :

![alt text](https://raw.githubusercontent.com/amitme10/EyesGazeRecognitionFove/master/heatMap.png)


![alt text](https://raw.githubusercontent.com/amitme10/EyesGazeRecognitionFove/master/chart.png)


# How was the Unity Project built?

One of the most important goals of the Unity project is to get the eye gaze coordinates from the FOVE device.

After collecting the manipulated data (eye gaze positions and time ) we will send it to the firebase. The Firebase is a real-time database, owned by Google , which stores the data on the server. Send an HTTP POST requests in order to send the data.

Those requests are implemented in the *FirebaseLoggingService.cs* The game basically controlled by the file Neon.cs: In this file we start to count the time for each step and switch the steps as well as the object pictures. We are able to change the pictures (if we want to change the issue of the test) by replacing it with another picture with the same size . The time in which the steps were being changed, is captured in the configuration file. At this file we use the *CommonData.cs* class, which is responsible for saving a real-time definitions during the game, which helps us to manage the game process.

*foveInterface.Gazecast(OBJECT)* – built in function of the FOVE interface SDK which telling weather eye gaze cursor is on the OBJECT.

we should pay attention to the sending of the coordinates vectors – this is the only sending that is being operated in the end of the game and not in real-time . that’s because sending a very large scale of data to the firebase would cause in a data fraction / loss of data.

# Configuration.cs:
The process of reading from the configuration file is being done here. The action is to read from the configuration file (which named "json-schema1.json" in the Assets folder) and to store the definitions in object that will be transformed to json.

# Neon class:
In this file we save the current step of the game (there are 2 different variables which saves it and they are for different use)

We can also find in this file the *"GameTime"* which will be used as the game name and determines the tables name in the firebase.

In this file we also store the corrections order of the objects at each step under the array "corrects".

There is a problem that came up while testing the game – when we look at a object in one step and the step is passed (and we are still looking at the object) so also the object is changes – therefore we must update the database – for this problem CommonData uses 3 different Boolean variables ( trespass , clipper , timePass ) – handled by Neon . Finally, when the all 7 steps will be done we will use the variable "done" to sign it.

# FOVE3DCursor.cs:
This file usually handles the cursors (used by the component of the right eye as well as the left) , by our definition we use this file also to tell the logger the current position of the specific eye .

# FOVELookSample.cs:
This file is used by the 2 objects. Here we deliver (by the built in logic) the position of the cursor and the times (generally) . The detection (which determines what position to send) takes part in this file – the logic is , we check if the cursor (of each eye) is on the object (each one of them) , rather or not this position (even if it is not on any object) – is counted and delivered to the special function that handles this data and after manipulate it send it to the firebase.

Functions we should pay attention to : Physics.Raycast(r, out hit, Mathf.Infinity); as you can notice there is out to the variable hit. We will use this variable after performing this function in order to get from it the x and y values of the eye gaze.

# FirebaseLoggingService.cs :
this file defines most of the data handling functions and the basic one (the actual writing to the firebase). all the data that sent is transformed to json as the firebase demands.

*public static IEnumerator sendFirebase(string category, string GameTime, Guid guid , System.Object myObject)* this function is the basic function that implements the writing data for the firebase , of course it does it with the GameTime to determine the name of the specific game table . All the functions that want to write the firebase will use this function.

*public static IEnumerator sendLocationsPerStep(location cordinate)* this function implemented here and used a lot of times (loop in the end oof the game) to send coordinate object to the firebase.

*public static IEnumerator sendTimes(string time, int step, string colliderName)* the function gets the time that we spent on a specific object (per step) even if there is no object (which mean just looking on the "wall" of the game) and send it to the firebase with unique symbol of the object (0 – for the right object . 2 - for the left object . 1 – for none ).

# Requirements
Unity 5.4.5f1 or newer
FOVE VR 0.12.1 or newer"# EyesGazeRecognitionFove"
