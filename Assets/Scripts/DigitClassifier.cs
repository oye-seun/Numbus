using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

public class DigitClassifier : MonoBehaviour
{
    private const int INPUT_SIZE = 784; // 28x28 pixels
    private const int HIDDEN_SIZE = 300;
    private const int OUTPUT_SIZE = 10; // 0-9 plus non-digit class

    private float[,] weightsInputHidden;
    private float[,] weightsHiddenOutput;
    private float[] biasHidden;
    private float[] biasOutput;


    void Awake()
    {
        InitializeNetwork();
        LoadWeights("Data/TransformedData.json");

        // Train the model
        // LoadMNISTTrainingData();
        // Train();
        // SaveWeights("Data/mnist_weights.json");
    }
    

    private void InitializeNetwork()
    {
        weightsInputHidden = new float[HIDDEN_SIZE, INPUT_SIZE];
        weightsHiddenOutput = new float[OUTPUT_SIZE, HIDDEN_SIZE];
        biasHidden = new float[HIDDEN_SIZE];
        biasOutput = new float[OUTPUT_SIZE];

        // Xavier initialization for better training
        float inputScale = Mathf.Sqrt(2.0f / INPUT_SIZE);
        float hiddenScale = Mathf.Sqrt(2.0f / HIDDEN_SIZE);

        for (int i = 0; i < HIDDEN_SIZE; i++)
        {
            for (int j = 0; j < INPUT_SIZE; j++)
            {
                weightsInputHidden[i, j] = UnityEngine.Random.Range(-inputScale, inputScale);
            }
            biasHidden[i] = 0;
        }

        for (int i = 0; i < OUTPUT_SIZE; i++)
        {
            for (int j = 0; j < HIDDEN_SIZE; j++)
            {
                weightsHiddenOutput[i, j] = UnityEngine.Random.Range(-hiddenScale, hiddenScale);
            }
            biasOutput[i] = 0;
        }
    }

    public (int digitClass, float confidence) Classify(Texture2D image)
    {
        // Convert image to input array
        float[] input = PreprocessImage(image);
        
        // Forward pass
        float[] hidden = new float[HIDDEN_SIZE];
        float[] output = new float[OUTPUT_SIZE];

        // Hidden layer
        for (int i = 0; i < HIDDEN_SIZE; i++)
        {
            float sum = biasHidden[i];
            for (int j = 0; j < INPUT_SIZE; j++)
            {
                sum += input[j] * weightsInputHidden[i, j];
            }
            hidden[i] = ReLU(sum);
        }

        // Output layer
        for (int i = 0; i < OUTPUT_SIZE; i++)
        {
            float sum = biasOutput[i];
            for (int j = 0; j < HIDDEN_SIZE; j++)
            {
                sum += hidden[j] * weightsHiddenOutput[i, j];
            }
            output[i] = sum;
        }

        // Apply softmax
        float[] probabilities = Softmax(output);
        
        // Get prediction
        int predictedClass = 0;
        float maxProbability = probabilities[0];
        
        for (int i = 1; i < OUTPUT_SIZE; i++)
        {
            if (probabilities[i] > maxProbability)
            {
                maxProbability = probabilities[i];
                predictedClass = i;
            }
        }

        return (predictedClass, maxProbability);
    }

    public static float[] PreprocessImage(Texture2D image)
    {
        // Resize to 28x28 if needed
        if (image.width != 28 || image.height != 28)
        {
            TextureScale.Bilinear(image, 28, 28);
        }

        float[] input = new float[INPUT_SIZE];
        Color[] pixels = image.GetPixels(0,0,28,28);

        // (Vector2Int minBound, Vector2Int maxBound) = HelperFunctions.GetTextureBounds(image);

        float min = 1;
        float max = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            // Convert to grayscale and normalize to [0,1]
            float val = (pixels[i].r + pixels[i].g + pixels[i].b) / 3.0f;
            if(val < min ) min = val;
            if(val > max ) max = val;
            input[i] = val;
        }

        // Vector2 vectorSum = Vector2.zero;
        // Vector2 centre = (minBound + maxBound)/2;
        // for(int i = minBound.x; i <= maxBound.x; i++)
        // {
        //     for(int j = minBound.y; j <= maxBound.y; j++)
        //     {
        //         Color col = image.GetPixel(i, j);
        //         if(col.a * (col.r + col.g + col.b)/3 > 0.3f)
        //         {
        //             vectorSum += (centre - new Vector2(i, j));
        //         }
        //     }
        // }

        for(int i = 0; i < input.Length; i++ )
        {
            if((input[i] - min) / (max - min) > 0.1f)
                input[i] = 1;
            else
                input[i] = 0;
        }

        // input[784] = vectorSum.x;
        // input[785] = vectorSum.y;

        return input;
    }

    private float ReLU(float x)
    {
        return Mathf.Max(0, x);
    }

    private float[] Softmax(float[] x)
    {
        float[] output = new float[x.Length];
        float max = x[0];
        for (int i = 1; i < x.Length; i++)
        {
            if (x[i] > max) max = x[i];
        }

        float sum = 0;
        for (int i = 0; i < x.Length; i++)
        {
            output[i] = Mathf.Exp(x[i] - max);
            sum += output[i];
        }

        for (int i = 0; i < x.Length; i++)
        {
            output[i] /= sum;
        }

        return output;
    }


    



    [System.Serializable]
    public class TrainingData
    {
        public Texture2D image;
        public int label; // 0-9 for digits, 10 for non-digits
    }

    [SerializeField] private float learningRate = 0.01f;
    [SerializeField] private int epochs = 1;
    [SerializeField] private int batchSize = 32;
    
    // Training data
    public List<TrainingData> trainingSet { get; private set; }
    public List<TrainingData> validationSet { get; private set; }

    public void Train()
    {
        if (trainingSet == null || trainingSet.Count == 0)
        {
            Debug.LogError("No training data available!");
            return;
        }

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            float epochLoss = 0;
            
            // Shuffle training data
            trainingSet = trainingSet.OrderBy(x => UnityEngine.Random.value).ToList();

            // Process in batches
            for (int batchStart = 0; batchStart < trainingSet.Count; batchStart += batchSize)
            {
                int currentBatchSize = Mathf.Min(batchSize, trainingSet.Count - batchStart);
                float batchLoss = TrainBatch(batchStart, currentBatchSize);
                epochLoss += batchLoss;
            }

            epochLoss /= trainingSet.Count;
            Debug.Log($"Epoch {epoch + 1}/{epochs}, Loss: {epochLoss}");

            Debug.Log($"Epoch {epoch}; Training accuracy: {EvaluateAccuracy(trainingSet) * 100}, Testing accuracy: {EvaluateAccuracy(validationSet) * 100}");
        }
    }

    private float TrainBatch(int startIdx, int batchSize)
    {
        float batchLoss = 0;

        // Accumulate gradients
        float[,] gradWeightsIH = new float[HIDDEN_SIZE, INPUT_SIZE];
        float[,] gradWeightsHO = new float[OUTPUT_SIZE, HIDDEN_SIZE];
        float[] gradBiasHidden = new float[HIDDEN_SIZE];
        float[] gradBiasOutput = new float[OUTPUT_SIZE];

        for (int i = 0; i < batchSize; i++)
        {
            TrainingData data = trainingSet[startIdx + i];
            float[] input = PreprocessImage(data.image);
            float[] targetOutput = new float[OUTPUT_SIZE];
            targetOutput[data.label] = 1; // One-hot encoding

            // Forward pass
            float[] hidden = new float[HIDDEN_SIZE];
            float[] output = new float[OUTPUT_SIZE];

            // Hidden layer
            for (int h = 0; h < HIDDEN_SIZE; h++)
            {
                float sum = biasHidden[h];
                for (int j = 0; j < INPUT_SIZE; j++)
                {
                    sum += input[j] * weightsInputHidden[h, j];
                }
                hidden[h] = ReLU(sum);
            }

            // Output layer
            for (int o = 0; o < OUTPUT_SIZE; o++)
            {
                float sum = biasOutput[o];
                for (int h = 0; h < HIDDEN_SIZE; h++)
                {
                    sum += hidden[h] * weightsHiddenOutput[o, h];
                }
                output[o] = sum;
            }

            float[] softmaxOutput = Softmax(output);

            // Calculate loss
            float loss = 0;
            for (int o = 0; o < OUTPUT_SIZE; o++)
            {
                loss += -targetOutput[o] * Mathf.Log(softmaxOutput[o] + 1e-7f);
            }
            batchLoss += loss;

            // Backward pass
            // Output layer gradients
            float[] outputGradients = new float[OUTPUT_SIZE];
            for (int o = 0; o < OUTPUT_SIZE; o++)
            {
                outputGradients[o] = softmaxOutput[o] - targetOutput[o];
            }

            // Hidden layer gradients
            float[] hiddenGradients = new float[HIDDEN_SIZE];
            for (int h = 0; h < HIDDEN_SIZE; h++)
            {
                float sum = 0;
                for (int o = 0; o < OUTPUT_SIZE; o++)
                {
                    sum += outputGradients[o] * weightsHiddenOutput[o, h];
                }
                hiddenGradients[h] = sum * (hidden[h] > 0 ? 1 : 0); // ReLU derivative
            }

            // Accumulate gradients
            for (int h = 0; h < HIDDEN_SIZE; h++)
            {
                for (int j = 0; j < INPUT_SIZE; j++)
                {
                    gradWeightsIH[h, j] += hiddenGradients[h] * input[j];
                }
                gradBiasHidden[h] += hiddenGradients[h];
            }

            for (int o = 0; o < OUTPUT_SIZE; o++)
            {
                for (int h = 0; h < HIDDEN_SIZE; h++)
                {
                    gradWeightsHO[o, h] += outputGradients[o] * hidden[h];
                }
                gradBiasOutput[o] += outputGradients[o];
            }
        }

        // Update weights and biases with averaged gradients
        float scale = learningRate / batchSize;
        
        for (int h = 0; h < HIDDEN_SIZE; h++)
        {
            for (int j = 0; j < INPUT_SIZE; j++)
            {
                weightsInputHidden[h, j] -= gradWeightsIH[h, j] * scale;
            }
            biasHidden[h] -= gradBiasHidden[h] * scale;
        }

        for (int o = 0; o < OUTPUT_SIZE; o++)
        {
            for (int h = 0; h < HIDDEN_SIZE; h++)
            {
                weightsHiddenOutput[o, h] -= gradWeightsHO[o, h] * scale;
            }
            biasOutput[o] -= gradBiasOutput[o] * scale;
        }

        return batchLoss / batchSize;
    }


    public float EvaluateAccuracy(List<TrainingData> dataSet)
    {
        if (dataSet == null || dataSet.Count == 0)
            return 0f;

        int correctPredictions = 0;

        foreach (var data in dataSet)
        {
            var (predictedClass, _) = Classify(data.image);
            if (predictedClass == data.label)
            {
                correctPredictions++;
            }
        }

        return (float)correctPredictions / dataSet.Count;
    }
    // // Add methods to save and load the trained weights
    // public void SaveWeights(string filename)
    // {
    //     string json = JsonSerializer.Serialize(new NetworkData
    //     {
    //         weightsInputHidden = weightsInputHidden,
    //         weightsHiddenOutput = weightsHiddenOutput,
    //         biasHidden = biasHidden,
    //         biasOutput = biasOutput
    //     });
        
    //     System.IO.File.WriteAllText(Application.dataPath + "/" + filename, json);
    // }
    

    // public void LoadWeights(string filename)
    // {
    //     string path = Application.dataPath + "/" + filename;
    //     if (System.IO.File.Exists(path))
    //     {
    //         string json = System.IO.File.ReadAllText(path);
    //         NetworkData data = JsonSerializer.Deserialize<NetworkData>(json);
    //         weightsInputHidden = data.weightsInputHidden;
    //         weightsHiddenOutput = data.weightsHiddenOutput;
    //         biasHidden = data.biasHidden;
    //         biasOutput = data.biasOutput;
    //         Debug.Log("weight loaded");
    //     }
    // }

    public void SaveWeights(string filePath)
{
    NetworkData data = new NetworkData()
    {
        weightsInputHidden = SerializeArray2D(weightsInputHidden),
        weightsHiddenOutput = SerializeArray2D(weightsHiddenOutput),
        biasHidden = biasHidden,
        biasOutput = biasOutput
    };

    // string jsonString = JsonUtility.ToJson(new SerializationWrapper(weightsData));
    string jsonString = JsonSerializer.Serialize(data);
    File.WriteAllText(Application.dataPath + "/" + filePath, jsonString);
}

public void LoadWeights(string filePath)
{
    if (!File.Exists(Application.dataPath + "/" + filePath)){
        Debug.Log("File not found: " + filePath);
        return;
    }

    string jsonString = File.ReadAllText(Application.dataPath + "/" + filePath);
    // var wrapper = JsonUtility.FromJson<SerializationWrapper>(jsonString);
    NetworkData data = JsonSerializer.Deserialize<NetworkData>(jsonString);
    
    // weightsInputHidden = DeserializeArray2D(wrapper.data["weightsInputHidden"] as float[], HIDDEN_SIZE, INPUT_SIZE);
    // weightsHiddenOutput = DeserializeArray2D(wrapper.data["weightsHiddenOutput"] as float[], OUTPUT_SIZE, HIDDEN_SIZE);
    // biasHidden = wrapper.data["biasHidden"] as float[];
    // biasOutput = wrapper.data["biasOutput"] as float[];

    weightsInputHidden = DeserializeArray2D(data.weightsInputHidden, HIDDEN_SIZE, INPUT_SIZE);
    weightsHiddenOutput = DeserializeArray2D(data.weightsHiddenOutput, OUTPUT_SIZE, HIDDEN_SIZE);
    biasHidden = data.biasHidden;
    biasOutput = data.biasOutput;
}

private float[] SerializeArray2D(float[,] array)
{
    int rows = array.GetLength(0);
    int cols = array.GetLength(1);
    float[] flatArray = new float[rows * cols];
    
    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            flatArray[i * cols + j] = array[i, j];
        }
    }
    
    return flatArray;
}

private float[,] DeserializeArray2D(float[] flatArray, int rows, int cols)
{
    float[,] array2D = new float[rows, cols];
    
    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            array2D[i, j] = flatArray[i * cols + j];
        }
    }
    
    return array2D;
}




     public void LoadMNISTTrainingData()
    {
        string dataPath = Application.dataPath + "/Data/MNIST/";
        string trainImagesPath = dataPath + "train-images.idx3-ubyte";
        string trainLabelsPath = dataPath + "train-labels.idx1-ubyte";

        string validationImagesPath = dataPath + "t10k-images.idx3-ubyte";
        string validationLabelsPath = dataPath + "t10k-labels.idx1-ubyte";

        trainingSet = MNISTLoader.LoadMNISTData(trainImagesPath, trainLabelsPath);
        validationSet = MNISTLoader.LoadMNISTData(validationImagesPath, validationLabelsPath);
        Debug.Log($"Loaded {trainingSet.Count} training examples");
    }
    
}

// Helper class for texture scaling
public class TextureScale
{
    public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
    {
        Color[] newColors = new Color[newWidth * newHeight];
        float ratioX = ((float)tex.width) / newWidth;
        float ratioY = ((float)tex.height) / newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                float px = (x * ratioX);
                float py = (y * ratioY);
                newColors[y * newWidth + x] = tex.GetPixelBilinear(px / tex.width, py / tex.height);
            }
        }

        tex.Reinitialize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();
    }
}

[System.Serializable]
public class NetworkData
{
    [JsonInclude]
    public float[] weightsInputHidden;
    [JsonInclude]
    public float[] weightsHiddenOutput;
    [JsonInclude]
    public float[] biasHidden;
    [JsonInclude]
    public float[] biasOutput;
}



[Serializable]
public class SerializationWrapper
{
    public Dictionary<string, object> data;

    public SerializationWrapper(Dictionary<string, object> data)
    {
        this.data = data;
    }
}
