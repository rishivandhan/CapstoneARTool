import open3d as o3d
import numpy as np
import os

FILE_PATH = "./models/BoydPillarSimple.pcd"



def load_and_print_pcd(file_path):
	try:
		if not os.path.exists(file_path):
			raise FileNotFoundError(f"File '{file_path}' not found. Check the path!")
	except Exception as e:
		print(e)
		exit(0)

	# Load the PCD file
	pcd = o3d.io.read_point_cloud(FILE_PATH)
	
	# Print the point cloud
	print("Point Cloud:")
	print(pcd)
	
	# Print the first few points to verify
	points = np.asarray(pcd.points)
	print("First few points:")
	print(points[:10])  # Print first 10 points

	# Example list of points
	points_list = [
		[0.0, 0.0, 0.0],
		[1.0, 0.0, 0.0],
		[0.0, 1.0, 0.0],
		[0.0, 0.0, 1.0]
	]

	# Convert to NumPy array
	points_array = np.array(points_list)

	# Create an Open3D point cloud object
	pcd = o3d.geometry.PointCloud()

	# Assign points to the point cloud
	pcd.points = o3d.utility.Vector3dVector(points_array)

	# Print the point cloud information
	print(pcd)

	# Visualize the point cloud (optional)
	o3d.visualization.draw_geometries([pcd])

if __name__ == "__main__":
	load_and_print_pcd(FILE_PATH)