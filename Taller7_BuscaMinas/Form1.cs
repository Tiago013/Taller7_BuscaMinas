using System;
using System.Drawing;
using System.Windows.Forms;

namespace Taller7_BuscaMinas
{
    public partial class Form1 : Form
    {
        private const int tamanoTableroFacil = 10, conteoMinaFacil = 20; // Tamaño del tablero y numero de minas en la dificultad Facil
        private const int tamanoTableroIntermedio = 15, conteoMinaIntermedio = 30; // Tamaño del tablero y numero de minas en la dificultad Intermedia
        private const int tamanoTableroDificil = 20, conteoMinaDificil = 50; // Tamaño del tablero y numero de minas en la dificultad Dificil
        private int tamanoTablero, conteoMina, profundidadExpansion;
        private Button[,] Boton; // Botones que representan el tablero
        private int[,] campoMinado; // Matriz que contiene las minas y los números
        private bool[,] marcadasConBanderilla; // Matriz que contiene las banderillas que marcaran a las minas
        private bool juegoFinalizado; // Bandera para saber si el juego ha terminado
        private MenuStrip menuStrip; // Menu

        private Thread timerThread;
        private int elapsedTime;
        private Label timerLabel;
        private bool timerRunning;

        public Form1()
        {
            InitializeComponent();
            CrearMenu();
            IniciarJuego();
        }

        private void CrearMenu()
        {
            menuStrip = new MenuStrip();
            ToolStripMenuItem Menu = new ToolStripMenuItem("Juego");
            ToolStripMenuItem MenuFacil = new ToolStripMenuItem("Fácil");
            ToolStripMenuItem MenuMedio = new ToolStripMenuItem("Intermedio");
            ToolStripMenuItem MenuDificil = new ToolStripMenuItem("Difícil");

            MenuFacil.Click += (s, e) =>   EmpezarJuego(tamanoTableroFacil, conteoMinaFacil, 15);
            MenuMedio.Click += (s, e) => EmpezarJuego(tamanoTableroIntermedio, conteoMinaIntermedio, 10);
            MenuDificil.Click += (s, e) => EmpezarJuego(tamanoTableroDificil, conteoMinaDificil, 5);

            Menu.DropDownItems.Add(MenuFacil);
            Menu.DropDownItems.Add(MenuMedio);
            Menu.DropDownItems.Add(MenuDificil);
            menuStrip.Items.Add(Menu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void EmpezarJuego(int size, int minas, int profundidad)
        {
            tamanoTablero = size;
            conteoMina = minas;
            profundidadExpansion = profundidad;
            IniciarJuego();
        }

        // Inicializa el tablero y la lógica básica del juego
        private void IniciarJuego()
        {
            Boton = new Button[tamanoTablero, tamanoTablero];
            campoMinado = new int[tamanoTablero, tamanoTablero];
            marcadasConBanderilla = new bool[tamanoTablero, tamanoTablero];
            juegoFinalizado = false;
            elapsedTime = 0;
            timerRunning = true;

            this.Controls.Clear();
            this.Controls.Add(menuStrip); // Añadir el menú otra vez
            menuStrip.BringToFront();
            int ajustarY = menuStrip.Height + 40;

            timerLabel = new Label();
            timerLabel.Text = $"Tiempo: {elapsedTime} s";
            timerLabel.Location = new Point(10, menuStrip.Height + 10);
            timerLabel.Size = new Size(100, 30);
            this.Controls.Add(timerLabel);

            for (int x = 0; x < tamanoTablero; x++)
            {
                for (int y = 0; y < tamanoTablero; y++)
                {
                    Boton[x, y] = new Button();
                    Boton[x, y].Size = new Size(30, 30);
                    Boton[x, y].Location = new Point(x * 30, y * 30 + ajustarY);
                    Boton[x, y].MouseUp += Button_MouseUp;
                    this.Controls.Add(Boton[x, y]);
                }
            }


            UbicarMinas(); // Colocar minas aleatoriamente
            timerThread = new Thread(Temporizador);
            timerThread.Start();
        }
        private void Temporizador()
        {
            while (timerRunning && !juegoFinalizado)
            {
                Thread.Sleep(1000);
                elapsedTime++;
                ActualizarTemporizador();
            }
        }
        private void ActualizarTemporizador()
        {
            if (timerLabel.InvokeRequired)
            {
                timerLabel.Invoke(new Action(ActualizarTemporizador));
            }
            else
            {
                timerLabel.Text = $"Tiempo: {elapsedTime} s";

            }
        }
        // Coloca minas aleatoriamente en el campo
        private void UbicarMinas()
        {
            Random rand = new Random();
            int UbicarMinas = 0;
            while (UbicarMinas < conteoMina)
            {
                int x = rand.Next(tamanoTablero);
                int y = rand.Next(tamanoTablero);
                if (campoMinado[x, y] != -1)
                {
                    campoMinado[x, y] = -1; // -1 representa una mina
                    UbicarMinas++;
                    // Incrementar números alrededor de la mina
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (x + i >= 0 && x + i < tamanoTablero && y + j >= 0 && y + j < tamanoTablero && campoMinado[x + i, y + j] != -1)
                            {
                                campoMinado[x + i, y + j]++;
                            }
                        }
                    }
                }
            }
        }

        // Manejar el evento de clic en los botones
        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            if (juegoFinalizado) return;

            Button clickedButton = sender as Button;
            Point buttonLocation = clickedButton.Location;
            int x = buttonLocation.X / 30;
            int y = (buttonLocation.Y - menuStrip.Height - 40) / 30;

            if (e.Button == MouseButtons.Right)
            {
                if (!marcadasConBanderilla[x, y] && clickedButton.Text == "")
                {
                    clickedButton.Text = "🚩";
                    marcadasConBanderilla[x, y] = true;
                }
                else if (marcadasConBanderilla[x, y])
                {
                    clickedButton.Text = "";
                    marcadasConBanderilla[x, y] = false;
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (marcadasConBanderilla[x, y]) return;
                RevelarCasilla(x, y, clickedButton, profundidadExpansion);
            }
        }

        private void RevelarCasilla(int x, int y, Button clickedButton, int profundidad)
        {
            if (campoMinado[x, y] == -1)
            {
                clickedButton.BackColor = Color.Red;
                MessageBox.Show("Juego Finalizado");
                juegoFinalizado = true;
                timerRunning = false;
                RevelarMinas();
            }
            else if (campoMinado[x, y] == 0 && profundidad > 0)
            {
                clickedButton.Text = "";
                Desminado(x, y, profundidad - 1);
            }
            else
            {
                clickedButton.Text = campoMinado[x, y].ToString();
            }
            clickedButton.Enabled = false;
            VerificarVictoria();
        }
        private void VerificarVictoria()
        {
            bool victoria = true;

            for (int x = 0; x < tamanoTablero; x++)
            {
                for (int y = 0; y < tamanoTablero; y++)
                {
                    if (campoMinado[x, y] != -1 && Boton[x, y].Enabled)
                    {
                        victoria = false;
                        break;
                    }
                }
                if (!victoria) break;
            }

            // Si todas las casillas sin mina han sido reveladas, el jugador gana
            if (victoria)
            {
                juegoFinalizado = true;
                timerRunning = false;
                MessageBox.Show("¡Felicidades! Has ganado el juego.");
                RevelarMinas();
            }
        }


        private void Desminado(int x, int y, int profundidad)
        {
            if (x < 0 || x >= tamanoTablero || y < 0 || y >= tamanoTablero || !Boton[x, y].Enabled || profundidad <= 0)
            return;

            Boton[x, y].Enabled = false;

            if (campoMinado[x, y] == 0)
            {
                Boton[x, y].Text = "";
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            Desminado(x + i, y + j, profundidad - 1);
                        }
                    }
                }
            }
            else
            {
                Boton[x, y].Text = campoMinado[x, y].ToString();
            }
        }

        // Revelar todas las minas cuando el juego ha terminado
        private void RevelarMinas()
        {
            for (int x = 0; x < tamanoTablero; x++)
            {
                for (int y = 0; y < tamanoTablero; y++)
                {
                    if (campoMinado[x, y] == -1)
                    {
                        Boton[x, y].BackColor = Color.Red;
                        Boton[x, y].Text = "💣";
                    }
                }
            }
        }
    }
}