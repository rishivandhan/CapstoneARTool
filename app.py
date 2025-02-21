from flask import Flask, request, jsonify
import numpy as np
import open3d as o3d
import copy
from pycpd import RigidRegistration

app = Flask(__name__)
MODEL_PATH = "./servertools/models/Lab.pcd"

reference_pcd = o3d.io.read_point_cloud(MODEL_PATH)
	
@app.route('/', methods=['GET'])
def home():
	return "Server is up and running!"

@app.route('/testdata', methods=['POST'])
def testdata():
	try:
		data = request.json
		print(data)

	except Exception as e:
		return jsonify({"error": str(e)})
	
	return "Data received successfully."

@app.route('/icp', methods=['POST'])
def localize_icp():
	try:
		# Receive point cloud data as JSON
		data = request.json
		points = np.array(data["points"])  # Expecting list of [x, y, z] points

		# Convert to Open3D point cloud
		input_pcd = o3d.geometry.PointCloud()
		input_pcd.points = o3d.utility.Vector3dVector(points)

		# Perform ICP registration
		threshold = 0.02  # Set a distance threshold
		trans_init = np.eye(4)  # Initial transformation guess
		reg_p2p = o3d.pipelines.registration.registration_icp(
			input_pcd, reference_pcd, threshold, trans_init,
			o3d.pipelines.registration.TransformationEstimationPointToPoint()
		)

		# Extract transformation matrix
		transformation = reg_p2p.transformation.tolist()

		# code to visualize the allignment
		aligned_input = copy.deepcopy(input_pcd)
		aligned_input.transform(reg_p2p.transformation)

		# Visualize
		# Color the clouds for clarity:
		reference_pcd.paint_uniform_color([1, 0, 0])		# Reference in red
		aligned_input.paint_uniform_color([0, 1, 0])		# Transformed input in green
		input_pcd.paint_uniform_color([0, 0, 1])			# Original input in blue

		# Visualize both point clouds together
		o3d.visualization.draw_geometries(
			[reference_pcd, aligned_input, input_pcd],
			window_name="Alignment Visualization",
			width=800,
			height=600
		)

		# Return transformation data to Unity
		return jsonify({"transformation": transformation})
	except Exception as e:
		return jsonify({"error": str(e)})
	
@app.route('/cpd', methods=['POST'])
def localize_cpd():
	try:
		# Receive point cloud data as JSON
		data = request.json
		input_points = np.array(data["points"])  # Expecting list of [x, y, z] points

		print("registration")

		# Apply CPD rigid registration
		cpd = RigidRegistration(X=input_points, Y=np.asarray(reference_pcd.points))
		transformed_source, transformation_matrix = cpd.register()

		print("visualizing")

		# Input to PCD (for visualization)
		input_pcd = o3d.geometry.PointCloud()
		input_pcd.points = o3d.utility.Vector3dVector(input_points)

		# Visualize
		reference_pcd.paint_uniform_color([1, 0, 0])		# Reference in red
		transformed_source.paint_uniform_color([0, 1, 0])		# Transformed input in green
		input_pcd.paint_uniform_color([0, 0, 1])			# Original input in blue

		# Visualize both point clouds together
		o3d.visualization.draw_geometries(
			[reference_pcd, transformed_source, input_pcd],
			window_name="Alignment Visualization",
			width=800,
			height=600
		)

		# Return transformation data to Unity
		return jsonify({"transformation": "NOT IMPLEMENTED"})
	except Exception as e:
		return jsonify({"error": str(e)})
	
@app.route("/visualize", methods=['POST'])
def visualize():
	data = request.json

	points_array = np.array(data["data"])
	pcd = o3d.geometry.PointCloud()
	pcd.points = o3d.utility.Vector3dVector(points_array)
	o3d.visualization.draw_geometries([pcd])

	return 'Data received successfully.'


if __name__ == '__main__':
	app.run(debug=True, host='0.0.0.0', port=5000)