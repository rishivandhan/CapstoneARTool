# AR Markerless World Alignment Project

Nate Kite, Rishi Musuvathi, Edward Liu, Mihir Joshi

In collaboration with Gulfstream Aerospace

Built with Unity and some other stuff.

### Requirements
TODO: Unity version requirement?

Version requirements:

- Ubuntu
- Python 3.8
- [CUDA Toolkit 11.X](https://developer.nvidia.com/cuda-11-8-0-download-archive)

The following installations are required for our code (open3d must be installed on the correct version to ensure compatability with GeDi):

```
pip install flask
pip install open3d==0.15.2
pip install pycpd
pip install gdown
pip install tensorboard
pip install protobuf==3.20
```

[GeDi](https://github.com/fabiopoiesi/gedi) requires a particular version of pytorch, which we have downloaded and included in this repository.

`pip install servertools/gedi/torch-1.8.1-cp38-cp38-linux_x86_64.whl`

Next, install PointNet2.

```
pip install servertools/gedi/backbones/pointnet2_ops_lib/
```