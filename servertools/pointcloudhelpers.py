from pycpd import RigidRegistration
import numpy as np
import open3d as o3d
import torch

# Runs ICP on two point clouds
# Returns a transformation
def run_icp(source, target):

	threshold = 0.02  # Set a distance threshold
	trans_init = np.eye(4)  # Initial transformation guess
	reg_p2p = o3d.pipelines.registration.registration_icp(
		source, target, threshold, trans_init,
		o3d.pipelines.registration.TransformationEstimationPointToPoint()
	)

	# Extract transformation
	return reg_p2p.transformation

# Runs CPD on two point clouds
# Returns a transformation
def run_cpd(source, target):

	# Apply CPD rigid registration
	cpd = RigidRegistration(X=np.asarray(source.points), Y=np.asarray(target.points))
	transformation_matrix = cpd.register()

	# Return transformation data to Unity
	return transformation_matrix

# Visualizes up to three point clouds
def visualize(source=None, target=None, transformed=None):
	visualizable = []

	# Color point clouds
	# source is red, target is blue, transformed is green
	if source:
		source.paint_uniform_color([1, 0, 0])
		source.estimate_normals()
		visualizable.append(source)
	if target:
		target.paint_uniform_color([0, 0, 1])
		target.estimate_normals()
		visualizable.append(target)
	if transformed:
		transformed.paint_uniform_color([0, 1, 0])
		transformed.estimate_normals()
		visualizable.append(transformed)

	o3d.visualization.draw_geometries(
		visualizable,
		window_name="Alignment Visualization",
		width=1200,
		height=900
	)

# Runs GeDi on two point clouds
# Returns a transformation
def run_gedi(source, target, gedi):

	# NOTE: The following code was copied from demo.py in servertools/gedi
	# I don't know what ANY of this means, and we should study it carefully
	# This code includes downsampling and some other stuff, which we should
	# probably do on our own once we understand what's going on.

	print('Downsampling...')
		
	voxel_size = .01
	patches_per_pair = 5000

	source_inds = np.random.choice(np.asarray(source.points).shape[0], patches_per_pair, replace=False)
	target_inds = np.random.choice(np.asarray(target.points).shape[0], patches_per_pair, replace=False)

	source_points = torch.tensor(np.asarray(source.points)[source_inds]).float()
	target_points = torch.tensor(np.asarray(target.points)[target_inds]).float()

	# applying voxelisation to the point cloud
	source = source.voxel_down_sample(voxel_size)
	target = target.voxel_down_sample(voxel_size)

	_source = torch.tensor(np.asarray(source.points)).float()
	_target = torch.tensor(np.asarray(target.points)).float()

	print('GeDi is doing the GeDi thing...')

	# computing descriptors
	source_desc = gedi.compute(pts=source_points, pcd=_source)
	target_desc = gedi.compute(pts=target_points, pcd=_target)

	print('GeDi complete...')

	# preparing format for open3d ransac
	source_dsdv = o3d.pipelines.registration.Feature()
	target_dsdv = o3d.pipelines.registration.Feature()

	source_dsdv.data = source_desc.T
	target_dsdv.data = target_desc.T

	_source = o3d.geometry.PointCloud()
	_source.points = o3d.utility.Vector3dVector(source_points)
	_target = o3d.geometry.PointCloud()
	_target.points = o3d.utility.Vector3dVector(target_points)

	print('Applying RANSAC...')

	# applying ransac
	est_result01 = o3d.pipelines.registration.registration_ransac_based_on_feature_matching(
		_source,
		_target,
		source_dsdv,
		target_dsdv,
		mutual_filter=True,
		max_correspondence_distance=.02,
		estimation_method=o3d.pipelines.registration.TransformationEstimationPointToPoint(False),
		ransac_n=3,
		checkers=[o3d.pipelines.registration.CorrespondenceCheckerBasedOnEdgeLength(.9),
				o3d.pipelines.registration.CorrespondenceCheckerBasedOnDistance(.02)],
		criteria=o3d.pipelines.registration.RANSACConvergenceCriteria(50000, 1000))

	print('Alignment complete.')

	return(est_result01.transformation)

# Unpacks HTTP response and builds a point cloud
# Also responsible for mirroring the point cloud from Unity's LH coordinates to Open3d's RH system
def build_pcd(request):
	data = request.json

	points = np.array(data["points"])

	# Mirrors the point cloud
	# Necessary because Unity and Open3D have different coordinate systems
	points[:, 0] *= -1

	pcd = o3d.geometry.PointCloud()
	pcd.points = o3d.utility.Vector3dVector(points)

	return pcd