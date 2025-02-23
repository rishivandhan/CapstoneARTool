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
    cpd = RigidRegistration(X=np.asarray(source.points),
                            Y=np.asarray(target.points))
    transformation_matrix = cpd.register()

    # Return transformation data to Unity
    return transformation_matrix


def run_fpfh(source, target):
    # Downsample the point clouds
    source_down = source.voxel_down_sample(voxel_size=0.05)
    target_down = target.voxel_down_sample(voxel_size=0.05)

    # Estimate normals
    source_down.estimate_normals(
        search_param=o3d.geometry.KDTreeSearchParamHybrid(radius=0.1, max_nn=30))
    target_down.estimate_normals(
        search_param=o3d.geometry.KDTreeSearchParamHybrid(radius=0.1, max_nn=30))

    # Compute FPFH features
    print("Computing FPFH features")
    source_fpfh = o3d.pipelines.registration.compute_fpfh_feature(
        source_down,
        o3d.geometry.KDTreeSearchParamHybrid(radius=0.1, max_nn=30)
    )
    target_fpfh = o3d.pipelines.registration.compute_fpfh_feature(
        target_down,
        o3d.geometry.KDTreeSearchParamHybrid(radius=0.1, max_nn=30)
    )

    # Perform RANSAC registration
    print("Starting RANSAC")
    result = o3d.pipelines.registration.registration_ransac_based_on_feature_matching(
        source_down, target_down, source_fpfh, target_fpfh,
        mutual_filter=True,
        max_correspondence_distance=0.075,
        estimation_method=o3d.pipelines.registration.TransformationEstimationPointToPoint(
            False),
        ransac_n=4,
        checkers=[o3d.pipelines.registration.CorrespondenceCheckerBasedOnEdgeLength(0.9),
                  o3d.pipelines.registration.CorrespondenceCheckerBasedOnDistance(0.075)],
        criteria=o3d.pipelines.registration.RANSACConvergenceCriteria(
            4000000, 500)
    )

    print("fpfh Done")
    return result.transformation

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
