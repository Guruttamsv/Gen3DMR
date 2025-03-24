from flask import Flask, request, send_file
import os
import torch
import tqdm
import numpy as np
import trimesh
from shap_e.diffusion.sample import sample_latents
from shap_e.diffusion.gaussian_diffusion import diffusion_from_config
from shap_e.models.download import load_model, load_config
from shap_e.util.notebooks import decode_latent_mesh

app = Flask(__name__)

device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
xm = load_model('transmitter', device=device)
model = load_model('text300M', device=device)
diffusion = diffusion_from_config(load_config('diffusion'))
print("Loading Models Complete")

@app.route('/generate', methods=['POST'])
def generate_model():
    prompt = request.json.get('prompt')
    if prompt == "Checking Connection":
        return "Connected To Server!", 200  
    if not prompt:
        return "No prompt provided!", 400

    print(f"Starting model generation for prompt: {prompt}")
    batch_size = 1
    guidance_scale = 25.0
    
    print("Generating latents for 3D model...")
    latents = sample_latents(
        batch_size=batch_size,
        model=model,
        diffusion=diffusion,
        guidance_scale=guidance_scale,
        model_kwargs=dict(texts=[prompt] * batch_size),
        progress=True,
        clip_denoised=True,
        use_fp16=True,
        use_karras=True,
        karras_steps=256,
        sigma_min=0.05,
        sigma_max=50,
        s_churn=0,
    )

    filename = f"{prompt.replace(' ', '_')}_model.glb"
    print(f"Processing and saving the generated model to {filename}...")
    
    for i, latent in enumerate(tqdm.tqdm(latents, desc="Processing latents", unit="latent")):
        t = decode_latent_mesh(xm, latent).tri_mesh()

        # Extract RGB channels from vertex_channels
        r = t.vertex_channels['R']
        g = t.vertex_channels['G']
        b = t.vertex_channels['B']

        # Stack into (N, 3) array and normalize
        vertex_colors = np.stack([r, g, b], axis=-1)
        vertex_colors = (vertex_colors - vertex_colors.min()) / (vertex_colors.max() - vertex_colors.min())
        vertex_colors = np.clip(vertex_colors * 3.0, 0, 1)  # Boost brightness
        vertex_colors = (vertex_colors * 255).astype(np.uint8)

        # Create Trimesh object with colors
        mesh = trimesh.Trimesh(vertices=t.verts, faces=t.faces, vertex_colors=vertex_colors)
        mesh.export(filename)

    print(f"Model generation completed! File saved as {filename}")
    return send_file(filename, as_attachment=True)

if __name__ == '__main__':
    app.run(debug=False, host='0.0.0.0', port=5000)
