# Test Photos Directory

Place your poker chip photos here for testing in Unity Editor.

## Supported Formats
- PNG
- JPG/JPEG
- TGA

## Recommended Photo Specifications
- **Resolution**: 1920x1080 or higher
- **Format**: PNG (for best quality)
- **Content**: Clear photos of poker chip stacks
- **Lighting**: Good lighting, minimal shadows
- **Background**: Contrasting background (preferably dark)

## How to Use
1. Place your chip photos in this folder
2. In Unity Editor, select the PhotoTestManager component
3. Click "Load Photos from Resources" in the context menu
4. Set expected chip counts for each photo
5. Run the scene and test the region selection

## Example Photos Needed
- Single stack (5-10 chips)
- Multiple stacks
- Different chip colors
- Various stack heights
- Edge cases (very tall stacks, mixed stacks)

## Testing Workflow
1. **Load Photos**: Use PhotoTestManager to load photos
2. **Select Regions**: Use StackRegionSelector to select chip areas
3. **Test Height**: Use StackHeightEstimator to calculate chip counts
4. **Verify Results**: Compare calculated vs expected chip counts
5. **Export Results**: Use PhotoTestManager to export test results
