from pycpd import RigidRegistration
import numpy as np
import open3d as o3d

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
		visualizable.append(source)
	if target:
		target.paint_uniform_color([0, 0, 1])
		visualizable.append(target)
	if transformed:
		transformed.paint_uniform_color([0, 1, 0])
		visualizable.append(transformed)

	o3d.visualization.draw_geometries(
		visualizable,
		window_name="Alignment Visualization",
		width=1200,
		height=900
	)
