from flask import Flask, request, jsonify
import numpy as np
import open3d as o3d
import json
import copy

app = Flask(__name__)
MODEL_PATH = "./servertools/models/BoydPillarSimple.pcd"

reference_pcd = o3d.io.read_point_cloud(MODEL_PATH)

@app.route('/testdata', methods=['POST'])
def testdata():
	try:
		data = request.json
		print(data)

	except Exception as e:
		return jsonify({"error": str(e)})



@app.route('/localize', methods=['POST'])
def localize():
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

		'''
		# Color the clouds for clarity:
		ref_vis = copy.deepcopy(reference_pcd)
		ref_vis.paint_uniform_color([1, 0, 0])         # Reference in red
		aligned_input.paint_uniform_color(
			[0, 1, 0])     # Transformed input in green

		# Visualize both point clouds together
		o3d.visualization.draw_geometries(
			[ref_vis, aligned_input],
			window_name="Alignment Visualization",
			width=800,
			height=600
		)
		'''
		# Return transformation data to Unity
		return jsonify({"transformation": transformation})
	except Exception as e:
		return jsonify({"error": str(e)})


if __name__ == '__main__':
	app.run(host='0.0.0.0', port=5000)