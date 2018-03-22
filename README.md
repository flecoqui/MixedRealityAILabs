# Windows Mixed Reality Artificial Intelligence Labs


Overview
---------
This github repo contains sample applications used for the Windows Mixed Reality Artificial Intelligence Labs. 
Those applications are Windows Mixed Reality Applications running either on Hololens or Windows Mixed Reality Immersive headset using [Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/).


The Labs
---------
Below the list of Labs:<p/>
	- **Lab 1**: Two application MR_Translation using Translator Text, MR_SpeechTranslation using Speech-To-Text, Text-To-Speech and Translator Text API.</p>
	- **Lab 2**: Application using ComputerVision</p>
	- **Lab 3**: Application using LUIS</p>
	- **Lab 4**: Application using Face Recognition.</p>

Prerequisites
--------------

In order to build the applications associated with each lab, you need: 
1. A machine running Windows 10 Fall Creator Update (RS3)
2. [Visual Studio 2017](https://www.visualstudio.com/downloads/ )
3. [Windows 10 SDK Fall Creator Update](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk)
4. [Unity version 2017.2.1p2](https://unity3d.com/unity/qa/patch-releases). While  installing Unity, don't forget to select Windows Store ILCPP Scripting Backend, Windows Store .NET Scripting Backend options.

![](https://raw.githubusercontent.com/flecoqui/MixedRealityAILabs/master/Docs/options.png)

Building the applications
--------------------------

1. Start Unity, **Open** the folder where the project is installed on your machine.
2. Once the project is opened,select **File** \> **Build Settings** \>

![](https://raw.githubusercontent.com/flecoqui/MixedRealityAILabs/master/Docs/settings.png)

3. On the dialog box **Build Settings** select **Universal Windows Platform** and click on button **Switch platform**.
4. Click on **Unity C# projects** check box.
5. Click on button **Build**. Select or Create the folder where you want to store the Visual Studio solution. Unity is now generating the Visual Studio solution. After few seconds the solution is generated.
6. Double-Click on the solution file, the Visual Studio will automacially open the Visual Studio project.
7. On the tool bar, select **Debug**, **x86** and **Device**

![](https://raw.githubusercontent.com/flecoqui/MixedRealityAILabs/master/Docs/vs.png)

8. Press Ctrl+Shift+B, or select **Build** \> **Build Solution** to build the solution.


Debugging the applications
---------------------------

1. Connect your Hololens to your machine with the USB cable
2. Power-on the Hololens.
3. On the tool bar, select **Debug**, **x86** and **Device**

![](https://raw.githubusercontent.com/flecoqui/MixedRealityAILabs/master/Docs/vs.png)

4. To debug the application and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the application without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.


Next steps
-----------

Those Labs are based on Cognitive Services. Those Labs could be extended to support the following features:</p>
1.  Support of other Cognitive Services or other Azure Services.</p>
2.  Support of the latest version of Unity.</p>

