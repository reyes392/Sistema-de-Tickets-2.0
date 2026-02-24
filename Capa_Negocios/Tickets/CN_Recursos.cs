using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

//estas 3 referencias nos ayudaran a mandar un mail al usuario
using System.Net.Mail;
using System.Net;
using System.IO;

namespace Capa_Negocios.Tickets
{
    public class CN_Recursos
    {
        //aca generamos la clave
        public static string GenerarClave()
        {
            string clave = Guid.NewGuid().ToString("N").Substring(0, 6);
            return clave;

        }

        //encriptar la clave
        public static string ConvertirSha256(string texto)
        {
            StringBuilder sb = new StringBuilder();
            //usar la referencia de "System.security.cryptography"
            //encriptamos la clave
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));


                foreach (byte b in result)
                {
                    sb.Append(b.ToString("x2"));

                }

            }
            return sb.ToString();
        }


        // ===============================
        // GENERAR HASH + SALT
        // ===============================
        public static string GenerarHashConSalt(string texto)
        {
            // 1. Generar Salt aleatorio
            byte[] saltBytes = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            string salt = Convert.ToBase64String(saltBytes);

            // 2. Unir password + salt
            string combinado = texto + salt;

            // 3. Aplicar SHA256
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(combinado);
                byte[] hash = sha.ComputeHash(bytes);

                string hashFinal = Convert.ToBase64String(hash);

                // 4. Retornar SALT|HASH
                return $"{salt}|{hashFinal}";
            }
        }

        // ===============================
        // VERIFICAR PASSWORD
        // ===============================
        public static bool VerificarHashConSalt(string password, string hashBD)
        {
            try
            {
                // Separar SALT|HASH
                string[] partes = hashBD.Split('|');

                if (partes.Length != 2)
                    return false;

                string salt = partes[0];
                string hashGuardado = partes[1];

                // Recalcular
                string combinado = password + salt;

                using (SHA256 sha = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(combinado);
                    byte[] hash = sha.ComputeHash(bytes);

                    string hashActual = Convert.ToBase64String(hash);

                    return hashActual == hashGuardado;
                }
            }
            catch
            {
                return false;
            }
        }




        // Método para enviar correo usando servidor interno
        public static bool EnviarCorreoInterno(string correo, string asunto, string mensaje)
        {
            bool resultado = false;

            try
            {
                // Validar formato de correo
                if (string.IsNullOrWhiteSpace(correo) || !EsCorreoValido(correo))
                {
                    Console.WriteLine($"Correo no válido: {correo}");
                    return false;
                }

                MailMessage mail = new MailMessage();
                mail.To.Add(correo);
                mail.From = new MailAddress("soportetecnico@farmaciasaba.com", "Sistema de Soporte Saba Nicaragua");
                mail.Subject = asunto;
                mail.Body = mensaje;
                mail.IsBodyHtml = true;

                var smtp = new SmtpClient()
                {
                    Host = "192.168.222.17",
                    Port = 25,
                    EnableSsl = false,
                    Credentials = new NetworkCredential("soportetecnico@farmaciasaba.com", "Fsaba2014")
                };

                smtp.Send(mail);
                resultado = true;
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Error de formato en el correo: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enviando correo: " + ex.Message);
            }

            return resultado;
        }
        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }


    }
}
