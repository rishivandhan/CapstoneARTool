import open3d as o3d
import numpy as np
import json
import requests
import sys


def pcd_to_json(pcd):
    """Convert an Open3D PointCloud to JSON format."""
    points = np.asarray(pcd.points).tolist()  # Convert NumPy array to list
    return json.dumps({"points": points})  # Format as JSON


def visualize_pcd(pcd_file):
    """Visualize an Open3D PointCloud object."""
    try:
        pcd = o3d.io.read_point_cloud(pcd_file)
        o3d.visualization.draw_geometries([pcd])

    except Exception as e:
        print("Error visualizing point cloud:", str(e))
        return


def send_pcd_to_flask(pcd_file, server_url="http://localhost:5000/localize"):
    """Load a PCD file and send it to the Flask server."""
    try:
        # Load the point cloud file
        pcd = o3d.io.read_point_cloud(pcd_file)

        # Convert to JSON
        json_data = pcd_to_json(pcd)
        # Set header for JSON format
        headers = {"Content-Type": "application/json"}

        # Send request to Flask server
        response = requests.post(server_url, data=json_data, headers=headers)

        # Print response
        if response.status_code == 200:
            print("Server Response:", json.dumps(response.json(), indent=4))

        else:
            print("Error:", response.status_code, response.text)
    except Exception as e:
        print("Error loading or sending PCD file:", str(e))


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python send_pcd_to_flask.py <path_to_pcd_file>")
    else:
        visualize_pcd(sys.argv[1])
        send_pcd_to_flask(sys.argv[1])