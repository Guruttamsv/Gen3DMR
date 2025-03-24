#Gen3DMR - Text-to-3D in Mixed Reality 🚀

Gen3D is an innovative project that leverages AI to generate 3D models from text prompts and allows users to interact with these models in mixed reality (VR). The app demonstrates the power of AI and VR integration, providing a real-time, interactive experience where text input is transformed into 3D objects that can be rotated, grabbed, and examined in a virtual environment.

How it Works
	1.	Text-to-3D Model Generation:
The app uses Shap-E, an AI model, to generate 3D models from textual descriptions. Users input a prompt (e.g., “futuristic spaceship” or “a dragon”), and the model generates the corresponding 3D object based on the text.
	2.	Server Communication:
The process of text-to-3D generation happens on a local PC. The text input is sent to the server, which processes the request, generates the 3D model, and then sends the model to the Meta Quest 3 VR headset. This communication is handled using Flask, a lightweight Python framework, and Ngrok, a service that creates a secure tunnel for communication between devices.
	3.	Unity VR Integration:
The Meta Quest 3 headset is used to render and display the generated 3D model in virtual reality. Once the model is received, it is loaded into the Unity 3D environment, where the user can interact with it in real-time. The interaction includes actions like rotating, zooming in, grabbing, and manipulating the object in space.
	4.	Real-Time Interaction:
The user can manipulate the model in the VR environment. The app supports basic VR interactions like rotating, zooming, and grabbing the 3D object, allowing for an immersive experience. The interaction is powered by Unity’s XR Toolkit, which handles controller inputs and object interactions.
	5.	Cross-Device Communication:
	•	Flask Server: The server runs on the PC, processing the text prompts and generating 3D models via Shap-E.
	•	Ngrok: Ngrok exposes the local Flask server to the internet, enabling the Meta Quest 3 to send requests to the server and receive 3D models.
	•	Meta Quest 3: The VR device receives and displays the generated 3D model, where the user can interact with it in a mixed reality environment.
	6.	Additional Features:
	•	The app also includes a virtual keyboard within the VR environment for text input.
	•	Model Animation: Some models are animated, adding more interactivity to the experience.

Technologies Used
	•	Shap-E: AI model for generating 3D models from text prompts.
	•	Unity 3D: Platform used for creating the VR environment and handling object interactions.
	•	Meta Quest SDK: SDK for integrating Meta Quest 3 with Unity for VR rendering and interaction.
	•	Flask: Python web framework used for setting up the server that communicates between the PC and the VR device.
	•	Ngrok: Tool used to expose the local Flask server to the internet, enabling remote communication between devices.
	•	Python: Language used for backend development and handling AI model integration.

Future Enhancements
	•	Real-time Model Refinement: Implementing an interactive interface for refining 3D models in real-time.
	•	Expanded Interactions: Adding more sophisticated user interactions, such as object scaling and animations.
	•	Additional AI Models: Exploring other AI models for more complex and realistic 3D model generation.

How to Use
	1.	Enter a text prompt (e.g., “futuristic spaceship”, “robot dog”) in the virtual keyboard within the VR environment.
	2.	Wait for the 3D model to be generated based on the text input.
	3.	Interact with the 3D model in the VR environment by rotating, grabbing, or zooming in and out.
	4.	Experience the 3D model in an immersive VR space.
