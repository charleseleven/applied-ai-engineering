using System;

namespace IA_Aplicada_Semana1
{
    public class RedeNeuralSimples
    {
        private readonly int _inputNodes; // Número de neurônios na camada de entrada
        private readonly int _hiddenNodes; // Número de neurônios na camada oculta
        private readonly int _outputNodes; // Número de neurônios na camada de saída

        // Matrizes de pesos
        private readonly double[,] _weights_ih; // Matriz de pesos entre Input e Hidden (camada oculta)
        private readonly double[,] _weights_ho; // Matriz de pesos entre Hidden e Output (camada de saída)
        private readonly Random _random = new Random(); // Gerador de números aleatórios para inicializar os pesos

        // Construtor que recebe a arquitetura da rede
        public RedeNeuralSimples(int inputNodes, int hiddenNodes, int outputNodes)
        {
            // Armazena os parâmetros nos campos privados
            _inputNodes = inputNodes;
            _hiddenNodes = hiddenNodes;
            _outputNodes = outputNodes;

            _weights_ih = CreateMatrix(_hiddenNodes, _inputNodes); // Cria matriz de pesos input → hidden com dimensões [hiddenNodes x inputNodes]
            _weights_ho = CreateMatrix(_outputNodes, _hiddenNodes); // Cria matriz de pesos hidden → output com dimensões [outputNodes x hiddenNodes]
        }

        // Função para inicializar os pesos com valores aleatórios entre -1 e 1
        private double[,] CreateMatrix(int rows, int cols) // Cria uma matriz bidimensional de pesos com as dimensões especificadas
        {
            double[,] matrix = new double[rows, cols]; // Inicializa a matriz
            for (int i = 0; i < rows; i++) // Preenche cada elemento da matriz com um valor aleatório entre -1 e 1
            {
                for (int j = 0; j < cols; j++) // Loop interno para preencher as colunas
                {
                    matrix[i, j] = _random.NextDouble() * 2 - 1; // Valor aleatório entre -1 e 1
                }
            }
            return matrix; // Retorna a matriz preenchida com pesos aleatórios
        }

        // Função de Ativação Sigmoid (esmaga qualquer valor para um intervalo entre 0 e 1)
        private double Sigmoid(double x) // Aplica a função de ativação sigmoid para transformar o valor de entrada em um valor entre 0 e 1
        {
            return 1.0 / (1.0 + Math.Exp(-x)); // Fórmula da função sigmoid: 1 / (1 + e^(-x))
        }

        // Fase de Inferência (Feedforward)
        public double[] Predict(double[] inputArray)
        {
            // 1. Processando a Camada Oculta (Hidden Layer)
            double[] hidden = new double[_hiddenNodes];
            for (int i = 0; i < _hiddenNodes; i++)
            {
                double sum = 0;
                for (int j = 0; j < _inputNodes; j++)
                {
                    sum += inputArray[j] * _weights_ih[i, j]; // Input * Peso
                }
                hidden[i] = Sigmoid(sum); // Aplica a ativação
            }

            // 2. Processando a Camada de Saída (Output Layer)
            double[] outputs = new double[_outputNodes];
            for (int i = 0; i < _outputNodes; i++)
            {
                double sum = 0;
                for (int j = 0; j < _hiddenNodes; j++)
                {
                    sum += hidden[j] * _weights_ho[i, j]; // Hidden * Peso
                }
                outputs[i] = Sigmoid(sum); // Resultado final
            }

            return outputs;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando Motor de IA em C#...\n");

            // Arquitetura: 3 Inputs, 4 Neurônios Ocultos, 1 Output (Eficiência do Pace)
            var minhaIA = new RedeNeuralSimples(3, 4, 1);

            // Dados do treino: [Cadência alta(0.63), BPM moderado(0.4), Com Placa de Carbono(1.0)]
            double[] treinoBarigui = { 0.63, 0.4, 1.0 };

            double[] previsaoDeEficiencia = minhaIA.Predict(treinoBarigui);

            Console.WriteLine($"Predição de Eficiência do Pace: {previsaoDeEficiencia[0] * 100:F2}%");
            Console.WriteLine("\nObs: Como os pesos iniciam aleatórios, a cada execução o valor será diferente antes do treinamento real.");
        }
    }
}