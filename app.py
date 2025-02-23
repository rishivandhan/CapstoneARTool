from flask import Flask, request, jsonify
import numpy as np
import open3d as o3d
import copy
from servertools import pointcloudhelpers as tools

app = Flask(__name__)
MODEL_PATH = "./servertools/models/table.pcd"

target_pcd = o3d.io.read_point_cloud(MODEL_PATH)


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
        # Build point cloud
        data = request.json
        source = o3d.geometry.PointCloud()
        source.points = o3d.utility.Vector3dVector(np.array(data["points"]))

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
        # Build point cloud
        data = request.json
        source = o3d.geometry.PointCloud()
        source.points = o3d.utility.Vector3dVector(np.array(data["points"]))

        transformation = tools.run_cpd(source, target_pcd)

        # Visualize output
        transformation = tools.run_icp(source, target_pcd)
        aligned = o3d.transform(transformation)
        tools.visualize(source=source, target=target_pcd, transformed=aligned)

        # Return transformation data as json
        return jsonify({"transformation": transformation.tolist()})
    except Exception as e:
        return jsonify({"error": str(e)})


@app.route('/fpfh', methods=['POST'])
def localize_fpfh():
    try:
        # Build point cloud
        data = request.json
        source = o3d.geometry.PointCloud()
        source.points = o3d.utility.Vector3dVector(np.array(data["points"]))

        transformation = tools.run_fpfh(source, target_pcd)

        # Visualize output
        aligned = copy.deepcopy(source)
        aligned.transform(transformation)
        tools.visualize(source=source, target=target_pcd, transformed=aligned)

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


if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)
