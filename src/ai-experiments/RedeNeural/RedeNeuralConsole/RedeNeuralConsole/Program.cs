using System;

namespace AiExperiments.RiskPerceptron
{
    /// <summary>
    /// Implementação de um Perceptron de camada única do zero.
    /// </summary>
    public class Perceptron
    {
        private double[] _weights;
        private double _bias;
        private readonly double _learningRate;
        private readonly Random _random;

        public Perceptron(int inputCount, double learningRate = 0.1)
        {
            _learningRate = learningRate;
            _random = new Random();
            _weights = new double[inputCount];

            // Inicialização dos pesos e bias com valores aleatórios entre -1 e 1
            for (int i = 0; i < inputCount; i++)
            {
                _weights[i] = _random.NextDouble() * 2 - 1;
            }
            _bias = _random.NextDouble() * 2 - 1;
        }

        /// <summary>
        /// Função de Ativação: Step Function (Função Degrau)
        /// Retorna 1 se o valor for maior ou igual a 0, caso contrário retorna 0.
        /// </summary>
        private int StepFunction(double sum)
        {
            return sum >= 0 ? 1 : 0;
        }

        /// <summary>
        /// Fase de Inferência (Feedforward)
        /// </summary>
        public int Predict(double[] inputs)
        {
            double sum = _bias; // O somatório inicia com o valor do bias

            for (int i = 0; i < inputs.Length; i++)
            {
                sum += inputs[i] * _weights[i];
            }

            return StepFunction(sum);
        }

        /// <summary>
        /// Fase de Treinamento
        /// Ajusta os pesos e o bias iterando sobre as épocas.
        /// </summary>
        public void Train(double[][] trainingInputs, int[] expectedOutputs, int epochs)
        {
            Console.WriteLine("--- Iniciando Treinamento do Perceptron ---");
            for (int epoch = 1; epoch <= epochs; epoch++)
            {
                int errorCount = 0;

                for (int i = 0; i < trainingInputs.Length; i++)
                {
                    int prediction = Predict(trainingInputs[i]);
                    int error = expectedOutputs[i] - prediction;

                    // Se houver erro, ajustamos os pesos e o bias
                    if (error != 0)
                    {
                        for (int j = 0; j < _weights.Length; j++)
                        {
                            _weights[j] += _learningRate * error * trainingInputs[i][j];
                        }
                        _bias += _learningRate * error;
                        errorCount += Math.Abs(error);
                    }
                }

                Console.WriteLine($"Época {epoch:D3} | Ajustes realizados: {errorCount}");

                // Se não houver erros na época, o modelo convergiu e aprendeu o padrão
                if (errorCount == 0)
                {
                    Console.WriteLine("Convergência alcançada! Treinamento finalizado antecipadamente.\n");
                    break;
                }
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Motor de IA - Laboratório do Perceptron de Risco\n");

            // Instancia o modelo: 2 Inputs, Taxa de aprendizado de 0.1
            Perceptron riscoPerceptron = new Perceptron(2, 0.1);

            // Conjunto de Dados Estáticos (Hardcoded)
            // Inputs: [Complexidade da Tarefa, Sobrecarga do Dev]
            double[][] trainingData = new double[][]
            {
                new double[] { 0, 0 }, // Tarefa simples, Dev tranquilo
                new double[] { 0, 1 }, // Tarefa simples, Dev sobrecarregado
                new double[] { 1, 0 }, // Tarefa complexa, Dev tranquilo
                new double[] { 1, 1 }  // Tarefa complexa, Dev sobrecarregado
            };

            // Outputs Esperados (Risco de Atraso): Comporta-se como uma porta lógica OR (Ou)
            // Atrasará se a tarefa for complexa OU o dev estiver sobrecarregado
            int[] expectedOutputs = { 0, 1, 1, 1 };

            // Treina o modelo por no máximo 20 épocas
            riscoPerceptron.Train(trainingData, expectedOutputs, 20);

            // Validação das Previsões após o treinamento
            Console.WriteLine("--- Teste de Previsões Após Treinamento ---");
            for (int i = 0; i < trainingData.Length; i++)
            {
                int riscoPrevisto = riscoPerceptron.Predict(trainingData[i]);

                string complexidade = trainingData[i][0] == 1 ? "Alta" : "Baixa";
                string sobrecarga = trainingData[i][1] == 1 ? "Sim" : "Não";
                string statusRisco = riscoPrevisto == 1 ? "ALTO (1)" : "BAIXO (0)";

                Console.WriteLine($"Complexidade: {complexidade,-5} | Sobrecarga: {sobrecarga,-3} -> Risco Previsto: {statusRisco}");
            }
        }
    }
}