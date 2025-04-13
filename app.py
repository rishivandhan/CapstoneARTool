from flask import Flask, request, jsonify, render_template
import numpy as np
import open3d as o3d
import sys
import copy
from servertools import pointcloudhelpers as tools
from servertools.gedi.gedi import GeDi

sys.path.append("./severtools/gedi/backbones")

app = Flask(__name__)

GEDI_CONFIG = {'dim': 32,                                               # descriptor output dimension
                'samples_per_batch': 500,                               # batches to process the data on GPU
                'samples_per_patch_lrf': 4000,                          # num. of point to process with LRF
                'samples_per_patch_out': 512,                           # num. of points to sample for pointnet++
                'r_lrf': .5,                                            # LRF radius
                'fchkpt_gedi_net': './servertools/gedi/data/chkpts/3dmatch/chkpt.tar'}      # path to checkpoint

gedi = None
model_pcd = None
model_path = None
visualize = False

counter = 0

@app.route('/', methods=['GET'])
def home():
    return render_template("control.html", model_path=model_path if model_path else "None", gedi=(gedi != None), visualize=visualize)

@app.route('/testdata', methods=['POST'])
def testdata():
    try:
        data = request.json
        print(data)

    except Exception as e:
        return jsonify({"error": str(e)})
    
    return "Data received successfully."
    
@app.route('/gedi', methods=['POST'])
def localize_gedi():
    if not model_pcd:
        print("Model has not been loaded!")
        return "Model has not been loaded!", 500
    if not gedi:
        print("GeDi has not been loaded!")
        return "GeDi has not been loaded!", 500

    try:
        print("Beginning alignment with GeDi...")
        scan_pcd = tools.build_pcd(request)
        transformation = tools.run_gedi(model_pcd, scan_pcd, gedi)

        print("Transformation matrix acquired: ")
        print(transformation)

        # Visualize output
        if visualize:
            print("Visualizing output...")
            aligned = copy.deepcopy(model_pcd)
            aligned.transform(transformation)
            tools.visualize(source=model_pcd, target=scan_pcd, transformed=aligned)

        print("Sending response to iPad.")

        # Return transformation data as json
        return jsonify({"transformation": transformation.tolist()})
    except Exception as e:
        print(str(e))
        return jsonify({"error": str(e)})
    
@app.route("/visualize_scan", methods=['POST'])
def visualize_scan():
    data = request.json

    points_array = np.array(data["data"])
    pcd = o3d.geometry.PointCloud()
    pcd.points = o3d.utility.Vector3dVector(points_array)
    o3d.visualization.draw_geometries([pcd])

    return 'Data received successfully.'

@app.route("/visualize_model", methods=['POST', 'GET'])
def visualize_model():
    o3d.visualization.draw_geometries([model_pcd])

    return 'Model visualized successfully.'


@app.route('/transformation_test', methods=['POST'])
def transformation_test():
    transformation = [[1.0, 0.0, 0.0, 0.0],
              [0.0, 1.0, 0.0, 0.0],
              [0.0, 0.0, 1.0, 0.0],
              [0.0, 0.0, 0.0, 1.0]]

    return jsonify({"transformation": transformation})

@app.route('/save_pcd', methods=['POST'])
def save_pcd():
    pcd = tools.build_pcd(request)

    # Save to servertools/output/out.pcd
    o3d.io.write_point_cloud("./servertools/output/out.pcd", pcd)

    # Return identity transformation so the app doesn't break
    transformation = [[1.0, 0.0, 0.0, 0.0],
            [0.0, 1.0, 0.0, 0.0],
            [0.0, 0.0, 1.0, 0.0],
            [0.0, 0.0, 0.0, 1.0]]
    return jsonify({"transformation": transformation})

@app.route('/load_gedi', methods=['GET', 'POST'])
def load_gedi():
    print('Loading GeDi...')
    
    global gedi
    gedi = GeDi(config=GEDI_CONFIG)

    print('GeDi is loaded. Ready to localize.')
    return 'GeDi is loaded. Ready to localize.'

@app.route('/toggle_visualization', methods=['POST'])
def toggle_visualization():
    global visualize
    visualize = not visualize

    return f'Visualization set to "{visualize}"', 200

@app.route('/change_model', methods=['POST'])
def change_model():
    global model_pcd, model_path

    scale = 0.0254 if request.json["scale"] else 1.0
    model_pcd = tools.obj_to_pcd(f'servertools/models/{request.json["userText"]}', scale)

    if not model_pcd:
        return 'WARNING: Model was not loaded properly!', 400
    
    model_path = request.json["userText"]
    return f'Model "{request.json["userText"]}" loaded successfully!'

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)