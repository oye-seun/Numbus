using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MNISTLoader
{
    private const int IMAGE_MAGIC = 2051;
    private const int LABEL_MAGIC = 2049;

    public static List<DigitClassifier.TrainingData> LoadMNISTData(string imagePath, string labelPath)
    {
        List<DigitClassifier.TrainingData> trainingData = new List<DigitClassifier.TrainingData>();
        
        try
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            byte[] labelBytes = File.ReadAllBytes(labelPath);

            // Read image file header
            int imageMagic = ReadBigInt32(imageBytes, 0);
            int imageCount = ReadBigInt32(imageBytes, 4);
            int rows = ReadBigInt32(imageBytes, 8);
            int cols = ReadBigInt32(imageBytes, 12);

            // Read label file header
            int labelMagic = ReadBigInt32(labelBytes, 0);
            int labelCount = ReadBigInt32(labelBytes, 4);

            // Verify magic numbers and counts match
            if (imageMagic != IMAGE_MAGIC || labelMagic != LABEL_MAGIC)
            {
                Debug.LogError("Invalid MNIST file format");
                return trainingData;
            }

            if (imageCount != labelCount)
            {
                Debug.LogError("Image and label counts don't match");
                return trainingData;
            }

            // Read data
            int imageOffset = 16; // After header
            int labelOffset = 8;  // After header

            for (int i = 0; i < imageCount; i++)
            {
                // Create texture for this digit
                Texture2D texture = new Texture2D(cols, rows, TextureFormat.R8, false);
                Color32[] pixels = new Color32[rows * cols];

                // // Read image pixels
                // for (int y = 0; y < rows; y++)
                // {
                //     for (int x = 0; x < cols; x++)
                //     {
                //         byte pixel = imageBytes[imageOffset + i * (rows * cols) + y * cols + x];
                //         pixels[y * cols + x] = new Color32(pixel, pixel, pixel, 255);
                //     }
                // }

                // Read image pixels - modified to flip vertically
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        byte pixel = imageBytes[imageOffset + i * (rows * cols) + y * cols + x];
                        // Flip the y-coordinate by reading from bottom to top
                        int flippedY = (rows - 1 - y);
                        pixels[flippedY * cols + x] = new Color32(pixel, pixel, pixel, 255);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();

                // Read corresponding label
                int label = labelBytes[labelOffset + i];

                trainingData.Add(new DigitClassifier.TrainingData
                {
                    image = texture,
                    label = label
                });

                if ((i + 1) % 1000 == 0)
                {
                    Debug.Log($"Loaded {i + 1} images...");
                }
            }

            Debug.Log($"Successfully loaded {trainingData.Count} MNIST images and labels");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading MNIST data: {e.Message}");
        }

        return trainingData;
    }

    // MNIST files store integers in big-endian format
    private static int ReadBigInt32(byte[] data, int offset)
    {
        return (data[offset] << 24) | (data[offset + 1] << 16) 
             | (data[offset + 2] << 8) | data[offset + 3];
    }
}



