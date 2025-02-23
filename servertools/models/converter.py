import open3d as o3d
import os


def main():
    input_path = input("Enter input filename: ")
    output_path = input("Enter output filename: ")

    convert(input_path, output_path)


def convert(input_path, output_path):

    try:
        if not os.path.exists(input_path):
            raise FileNotFoundError(
                f"File '{input_path}' not found. Check the path!")
    except Exception as e:
        print(e)
        exit(0)

    # Load the OBJ file
    mesh = o3d.io.read_triangle_mesh(input_path)

    # Check if the mesh has vertex normals (helps in better sampling)
    if not mesh.has_vertex_normals():
        mesh.compute_vertex_normals()

    # Sample points uniformly from the mesh surface
    num_points = 100000  # Increase for a denser cloud
    pcd = mesh.sample_points_uniformly(number_of_points=num_points)

    # Save as a PCD file
    o3d.io.write_point_cloud(output_path, pcd)

    print(f"Converted {input_path} to {output_path}.")


main()
