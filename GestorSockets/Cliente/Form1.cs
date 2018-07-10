using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GestorSockets
{
    public partial class frmCliente : Form
    {
        ConexionCliente gestorCliente = null;
        byte[] bufferTx;
        byte[] bufferRx = new byte[2048];
        public frmCliente()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void EstablecerGestorCliente(ConexionCliente gestor)
        {
            this.gestorCliente = gestor;
        }
        public void ManejoLog(string msg)
        {
            txtLog.AppendText(msg);
        }

        private void btnActualizarBinario_Click(object sender, EventArgs e)
        {
            bufferTx = null; txtBinarioEnviar.Text = "";
            String datosAEnviar = "";
            int posicion = -1;
            if (chkTexto.Checked && txtTextoAEnviar.Text.Length != 0)
            {
                if (txtTextoAEnviar.Text.IndexOf("\\n") != -1)
                {
                    String[] subs = txtTextoAEnviar.Text.Split(new String[] { "\\n" }, StringSplitOptions.None);
                    for (int i = 0; i < subs.Length; i++)
                    {
                        if (subs[i].Equals(""))
                            datosAEnviar += "\n";
                        else datosAEnviar += subs[i];
                    }
                }
                else
                    datosAEnviar = txtTextoAEnviar.Text;
                bufferTx = Encoding.ASCII.GetBytes(datosAEnviar);
            }
            else
            {
                if (txtTextoAEnviar.Text.Length >= 2)
                {
                    string delimitador = " ";
                    byte resultado = 0x00; char[] numero;
                    string[] cadenaDeDatos = txtTextoAEnviar.Text.Split(delimitador.ToCharArray());
                    bufferTx = new byte[cadenaDeDatos.Length];
                    for (int i = 0; i < cadenaDeDatos.Length; i++)
                    {
                        try
                        {
                            numero = cadenaDeDatos[i].ToCharArray();
                            if (numero.Length != 2)
                                throw new Exception("");
                            byte.TryParse(numero[0].ToString(), out resultado);
                            bufferTx[i] = (byte)(resultado << 4);
                            byte.TryParse(numero[1].ToString(), out resultado);
                            bufferTx[i] |= (byte)resultado;
                        }
                        catch (Exception ex)
                        {
                            gestorCliente.Traza("Hay un error en el formato, recuerda: si es binario, debes escribir, 12 34 AB, siendo estos numeros hexadecimales");
                        }
                    }
                }
            }

            if (bufferTx != null) for (int i = 0; i < bufferTx.Length; i++)
                {
                    txtBinarioEnviar.AppendText(bufferTx[i].ToString("x") + " ");
                }
        }

        private void btnResolver_Click(object sender, EventArgs e)
        {
            if (txtServidor.Text.Length > 0)
            {
                txtIPServidor.Text = gestorCliente.ObtenerDireccionIP(txtServidor.Text).ToString();
            }
            else gestorCliente.Traza("Por favor incluye el nombre del servidor");
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtIPServidor.Text.Length != 0 && txtServidor.Text.Length != 0 && txtPuerto.Text.Length != 0)
                {
                    gestorCliente.EspecificarServidor(txtServidor.Text);
                    gestorCliente.EspecificarPuertoServidor(txtPuerto.Text);
                    gestorCliente.Conectar();
                }
                else throw new Exception();
            }
            catch (Exception ex)
            {
                gestorCliente.Traza("Por favor comprueba la dirección IP o el nombre de servidor");
            }
        }
        private void btnEnviar_Click(object sender, EventArgs e)
        {
            int recibidos = gestorCliente.EnviarRecibir(bufferTx, ref bufferRx);
            gestorCliente.Traza("Recibidos: " + recibidos + " bytes");
            txtRecibidoBinario.Text = "";
            for (int i = 0; i < recibidos; i++)
            {
                txtRecibidoBinario.AppendText(bufferRx[i].ToString("X"));
            }
            String respuesta = Encoding.ASCII.GetString(bufferRx, 0, recibidos);
            txtRespuesta.Text = "";
            txtRespuesta.AppendText(respuesta);
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            lblEstado.Text = gestorCliente.ObtenerResultadoPruebaConexion();
        }

    }
}
