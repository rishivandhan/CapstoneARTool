from flask import Flask, request, jsonify
import numpy as np
import open3d as o3d
import copy
import sys
from servertools import pointcloudhelpers as tools
from servertools.gedi.gedi import GeDi

sys.path.append("./severtools/gedi/backbones")

app = Flask(__name__)

MODEL_PATH = "./servertools/models/TunnelCAD.pcd"
target_pcd = o3d.io.read_point_cloud(MODEL_PATH)

GEDI_CONFIG = {'dim': 32,												# descriptor output dimension
				'samples_per_batch': 500,								# batches to process the data on GPU
				'samples_per_patch_lrf': 4000,							# num. of point to process with LRF
				'samples_per_patch_out': 512,							# num. of points to sample for pointnet++
				'r_lrf': .5,											# LRF radius
				'fchkpt_gedi_net': './servertools/gedi/data/chkpts/3dmatch/chkpt.tar'}		# path to checkpoint

gedi = GeDi(config=GEDI_CONFIG)
	

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
		source = tools.build_pcd(request)
		transformation = tools.run_icp(source, target_pcd)

		# Visualize output
		aligned = copy.deepcopy(source)
		aligned.transform(transformation)
		tools.visualize(source=source, target=target_pcd, transformed=aligned)

		# Return transformation data as json
		return jsonify({"transformation": transformation.tolist()})
	except Exception as e:
		print(str(e))
		return jsonify({"error": str(e)})
	
@app.route('/cpd', methods=['POST'])
def localize_cpd():
	try:
		source = tools.build_pcd(request)
		transformation = tools.run_cpd(source, target_pcd)

		# Visualize output
		transformation = tools.run_icp(source, target_pcd)
		aligned = o3d.transform(transformation)
		tools.visualize(source=source, target=target_pcd, transformed=aligned)

		# Return transformation data as json
		return jsonify({"transformation": transformation.tolist()})
	except Exception as e:
		return jsonify({"error": str(e)})
	
@app.route('/gedi', methods=['POST'])
def localize_gedi():
	try:
		source = tools.build_pcd(request)
		transformation = tools.run_gedi(source, target_pcd, gedi)

		# Visualize output
		aligned = copy.deepcopy(source)
		aligned.transform(transformation)
		tools.visualize(source=aligned, target=target_pcd)

		# Return transformation data as json
		return jsonify({"transformation": transformation.tolist()})
	except Exception as e:
		print(str(e))
		return jsonify({"error": str(e)})
	
@app.route("/visualize", methods=['POST'])
def visualize():
	data = request.json

	points_array = np.array(data["data"])
	pcd = o3d.geometry.PointCloud()
	pcd.points = o3d.utility.Vector3dVector(points_array)
	o3d.visualization.draw_geometries([pcd])

	return 'Data received successfully.'


@app.route('/transformation_test', methods=['POST'])
def transformation_test():
    transformation = [[1.0, 0.0, 0.0, 0.0],
              [0.0, 1.0, 0.0, 0.0],
              [0.0, 0.0, 1.0, 0.0],
              [0.0, 0.0, 0.0, 1.0]]

    return jsonify({"transformation": transformation})

@app.route('/save_pcd', methods=['POST'])
def save_pcd():
	data = request.json

	# Build pcd
	points_array = np.array(data["data"])
	pcd = o3d.geometry.PointCloud()
	pcd.points = o3d.utility.Vector3dVector(points_array)

	# Save to servertools/output/out.pcd
	o3d.io.write_point_cloud("./servertools/output/out.pcd", pcd)

	# Return identity transformation so the app doesn't break
	transformation = [[1.0, 0.0, 0.0, 0.0],
			[0.0, 1.0, 0.0, 0.0],
			[0.0, 0.0, 1.0, 0.0],
			[0.0, 0.0, 0.0, 1.0]]
	return jsonify({"transformation": transformation})

if __name__ == '__main__':
	app.run(debug=True, host='0.0.0.0', port=5000)