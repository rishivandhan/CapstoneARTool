# AR Markerless World Alignment Project

Nate Kite, Rishi Musuvathi, Edward Liu, Mihir Joshi

In collaboration with Gulfstream Aerospace

Built with Unity and some other stuff.

### Requirements

Version requirements:

- Ubuntu
- Python 3.8
- [CUDA Toolkit 11.X](https://developer.nvidia.com/cuda-11-8-0-download-archive)
- Unity 2022.3.47 (other versions might work, but this is what we used)

The following installations are required for our code:

```
pip install flask
pip install open3d==0.15.2
pip install pycpd
pip install gdown
pip install tensorboard
pip install protobuf==3.20
pip install torchgeometry==0.1.2
```

[GeDi](https://github.com/fabiopoiesi/gedi) requires a particular version of pytorch. You can get it [here](https://github.com/isl-org/open3d_downloads/releases/tag/torch1.8.1). Download it, then run:

`pip install torch-1.8.1-cp38-cp38-linux_x86_64.whl`

Next, install PointNet2.

```
pip install servertools/gedi/backbones/pointnet2_ops_lib/
```

### WSL Configuration

If you are running Flask inside WSL, you'll need to configure incoming traffic to be redirected to WSL. You can use this command:

```
netsh interface portproxy add v4tov4 listenport=5000 listenaddress=0.0.0.0 connectport=5000 connectaddress=<WSL IP>
```

You may also need to let this through your firewall.

References:
@inproceedings{Poiesi2021,
  title = {Learning general and distinctive 3D local deep descriptors for point cloud registration},
  author = {Poiesi, Fabio and Boscaini, Davide},
  booktitle = {IEEE Trans. on Pattern Analysis and Machine Intelligence},
  year = {(early access) 2022}
}