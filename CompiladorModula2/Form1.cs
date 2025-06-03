using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CompiladorModula2
{
    public partial class Form1 : Form
    {
        private List<string> erroresLexicos = new List<string>();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (dgvTablaSimbolos.Columns.Count == 0)
            {
                dgvTablaSimbolos.Columns.Add("Token", "ID");
                dgvTablaSimbolos.Columns.Add("Nombre", "Nombre");
                dgvTablaSimbolos.Columns.Add("Tipo", "Tipo");
                dgvTablaSimbolos.Columns.Add("Valor", "Valor");
                dgvTablaSimbolos.Columns.Add("Asignacion", "Asignación");
                dgvTablaSimbolos.Columns.Add("Referencia", "Referencia");
                dgvTablaSimbolos.Font = new Font("Century Gothic", 8.25f);
                dgvTablaSimbolos.ColumnHeadersDefaultCellStyle.Font = new Font("Century Gothic", 8.25f, FontStyle.Bold);


            }
        }

        public class VariableSimbolo
        {
            public string Token { get; set; }
            public string Nombre { get; set; }
            public string Tipo { get; set; } = "";
            public string Valor { get; set; } = "";
            public int LineaAsignacion { get; set; } = -1;
            public List<int> Referencias { get; set; } = new List<int>();
        }

        private Dictionary<string, VariableSimbolo> tablaSimbolos = new Dictionary<string, VariableSimbolo>();
        private Dictionary<int, int> contadorPorCategoria = new Dictionary<int, int>();
        private HashSet<string> palabrasReservadas = new HashSet<string>
        {
            "MODULE", "VAR", "BEGIN", "END", "INTEGER", "REAL", "BOOLEAN", "CHAR", "IF", "THEN", "TRUE", "FALSE"
        };

        private void btnAnalisisLexico_Click(object sender, EventArgs e)
        {
            AnalizarCodigo();
        }

        private void AnalizarCodigo()
        {
            dgvTablaSimbolos.Rows.Clear();
            lbTokens.Items.Clear();
            lbControl.Items.Clear();
            tablaSimbolos.Clear();
            contadorPorCategoria.Clear();
            List<string> erroresLexicos = new List<string>();  // <-- NUEVO

            string[] lineas = txtBloqueCodigo.Text.Split('\n');
            int numLinea = 1;

            // Etapa 1: Tabla de símbolos
            foreach (string raw in lineas)
            {
                string linea = raw.Trim();

                var declaraciones = Regex.Matches(linea, @"([a-zA-Z][a-zA-Z0-9]*)\s*:\s*(INTEGER|REAL|BOOLEAN|CHAR)");
                foreach (Match match in declaraciones)
                {
                    string nombre = match.Groups[1].Value;
                    string tipo = match.Groups[2].Value;
                    RegistrarVariable(nombre, tipo, numLinea);
                }

                var asignacion = Regex.Match(linea, @"^([a-zA-Z][a-zA-Z0-9]*)\s*:=\s*(.+);$");
                if (asignacion.Success)
                {
                    string lhs = asignacion.Groups[1].Value;
                    string rhs = asignacion.Groups[2].Value;
                    AsignarValor(lhs, rhs, numLinea);

                    var posibles = Regex.Split(rhs, @"[^a-zA-Z0-9]+");
                    foreach (var refNombre in posibles)
                    {
                        if (tablaSimbolos.ContainsKey(refNombre) && refNombre != lhs)
                        {
                            if (!tablaSimbolos[refNombre].Referencias.Contains(numLinea))
                                tablaSimbolos[refNombre].Referencias.Add(numLinea);
                        }
                    }
                }

                foreach (var simbolo in tablaSimbolos.Values)
                {
                    var regex = new Regex(@"\b" + Regex.Escape(simbolo.Nombre) + @"\b");
                    if (regex.IsMatch(linea) && !simbolo.Referencias.Contains(numLinea))
                    {
                        simbolo.Referencias.Add(numLinea);
                    }
                }

                numLinea++;
            }

            MostrarTablaSimbolos();

            // Etapa 2: Tokens léxicos con detección de errores
            contadorPorCategoria.Clear();
            numLinea = 1;

            foreach (string raw in lineas)
            {
                string linea = raw.Trim();
                string[] tokens = Regex.Split(linea, @"(\s+|:=|\.{2}|>=|<=|<>|=|#|<|>|\+|\-|\*|/|:|;|,|\.|\(|\)|\[|\]|\{|\})");

                foreach (string rawToken in tokens)
                {
                    string token = rawToken.Trim();
                    if (string.IsNullOrEmpty(token)) continue;

                    string upperToken = token.ToUpper();

                    if (palabrasReservadas.Contains(upperToken))
                        RegistrarToken(token, 5);
                    else if (upperToken == "AND" || upperToken == "OR" || upperToken == "NOT")
                        RegistrarToken(token, 40);
                    else if (Regex.IsMatch(token, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                    {
                        // Rechazar si es una palabra que se parece mucho a una reservada, pero no lo es
                        if (palabrasReservadas.Any(res => res.StartsWith(token) || token.StartsWith(res)) && !palabrasReservadas.Contains(token.ToUpper()))
                        {
                            erroresLexicos.Add($"[Línea {numLinea}] Error léxico: Token inválido '{token}' (posible palabra reservada mal escrita)");
                        }
                        else
                        {
                            RegistrarToken(token, 10);
                        }
                    }


                    else if (Regex.IsMatch(token, @"^\d+$"))
                        RegistrarToken(token, 15);
                    else if (Regex.IsMatch(token, @"^\d+\.\d+$"))
                        RegistrarToken(token, 15);
                    else if (Regex.IsMatch(token, @"^'.'$"))
                        RegistrarToken(token, 20);
                    else if (Regex.IsMatch(token, "^\".*\"$"))
                        RegistrarToken(token, 25);
                    else if ("+-*/".Contains(token))
                        RegistrarToken(token, 30);
                    else if (new[] { "=", "#", "<", ">", "<=", ">=", "<>" }.Contains(token))
                        RegistrarToken(token, 35);
                    else if (token == "..")
                        RegistrarToken(token, 45);
                    else if (token == ":=")
                        RegistrarToken(token, 50);
                    else if (token == ":")
                        RegistrarToken(token, 55);
                    else if (token == ";" || token == ",")
                        RegistrarToken(token, 60);
                    else if (token == ".")
                        RegistrarToken(token, 65);
                    else if (new[] { "{", "}", "(", ")", "[", "]" }.Contains(token))
                        RegistrarToken(token, 70);
                    else
                    {
                        erroresLexicos.Add($"[Línea {numLinea}] Error léxico: Token no reconocido '{token}'");
                    }
                }

                numLinea++;
            }

            // Mostrar errores léxicos
            if (erroresLexicos.Count > 0)
            {
                foreach (string error in erroresLexicos)
                    lbControl.Items.Add(error);
            }
            else
            {
                lbControl.Items.Add("✔ Análisis léxico completado sin errores.");
            }

            lbControl.Items.Add($"Líneas analizadas: {numLinea - 1}");

            // Etapa 3: Árbol sintáctico
            NodoArbol arbol = ConstruirArbol(lineas);
            MostrarArbol(arbol);
        }


        private void RegistrarVariable(string nombre, string tipo, int linea)
        {
            if (palabrasReservadas.Contains(nombre.ToUpper()) || nombre.ToUpper() == "SUMA")
                return;

            if (!tablaSimbolos.ContainsKey(nombre))
            {
                int cod = 10;
                if (!contadorPorCategoria.ContainsKey(cod))
                    contadorPorCategoria[cod] = 1;
                else
                    contadorPorCategoria[cod]++;

                string token = contadorPorCategoria[cod].ToString("D3") + cod.ToString("D2");

                tablaSimbolos[nombre] = new VariableSimbolo
                {
                    Token = token,
                    Nombre = nombre,
                    Tipo = tipo,
                    LineaAsignacion = linea,
                    Referencias = new List<int> { linea }
                };
            }
        }

        private void AsignarValor(string nombre, string valor, int linea)
        {
            if (tablaSimbolos.ContainsKey(nombre))
            {
                tablaSimbolos[nombre].Valor = valor;
                if (!tablaSimbolos[nombre].Referencias.Contains(linea))
                    tablaSimbolos[nombre].Referencias.Add(linea);
            }
        }

        private void RegistrarToken(string lexema, int categoria)
        {
            int secuencia = 1;
            if (contadorPorCategoria.ContainsKey(categoria))
                secuencia = ++contadorPorCategoria[categoria];
            else
                contadorPorCategoria[categoria] = secuencia;

            string token = secuencia.ToString("D3") + categoria.ToString("D2");

            bool yaExiste = lbTokens.Items.Cast<string>().Any(item => item.Contains($"{token} {lexema}"));
            if (!yaExiste)
                lbTokens.Items.Add($"{token} {lexema}");
        }

        private void MostrarTablaSimbolos()
        {
            dgvTablaSimbolos.Rows.Clear();
            foreach (var s in tablaSimbolos.Values)
            {
                string referencias = string.Join(",", s.Referencias.OrderBy(x => x));
                dgvTablaSimbolos.Rows.Add(
                    s.Token,
                    s.Nombre,
                    s.Tipo,
                    s.Valor,
                    s.LineaAsignacion == -1 ? "" : s.LineaAsignacion.ToString(),
                    referencias
                );
            }
        }

        // Árbol sintáctico
        private NodoArbol ConstruirArbol(string[] lineas)
        {
            NodoArbol raiz = new NodoArbol { Texto = "Programa" };
            NodoArbol bloque = new NodoArbol { Texto = "Bloque" };
            NodoArbol declaraciones = new NodoArbol { Texto = "Declaraciones" };
            NodoArbol sentencias = new NodoArbol { Texto = "Sentencias" };

            bool enDeclaraciones = false;
            bool enSentencias = false;

            Stack<NodoArbol> pilaBloques = new Stack<NodoArbol>();
            pilaBloques.Push(sentencias);

            foreach (string raw in lineas)
            {
                string linea = raw.Trim();
                if (string.IsNullOrWhiteSpace(linea)) continue;

                if (linea.StartsWith("MODULE"))
                {
                    var nodoModule = new NodoArbol { Texto = "MODULE" };
                    string[] partes = linea.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length >= 2)
                    {
                        nodoModule.Hijos.Add(new NodoArbol { Texto = "Identificador\n" });
                        nodoModule.Hijos.Add(new NodoArbol { Texto = partes[1] });
                        nodoModule.Hijos.Add(new NodoArbol { Texto = ";" });
                    }
                    raiz.Hijos.Add(nodoModule);
                    raiz.Hijos.Add(bloque);
                }
                else if (linea.StartsWith("VAR"))
                {
                    enDeclaraciones = true;
                    declaraciones.Hijos.Add(new NodoArbol { Texto = "VAR" });
                }
                else if (enDeclaraciones && Regex.IsMatch(linea, @"^[a-zA-Z][a-zA-Z0-9]*\s*:\s*(INTEGER|REAL|CHAR|BOOLEAN).*;"))
                {
                    var declaracion = new NodoArbol { Texto = "Declaracion" };
                    var m = Regex.Match(linea, @"^([a-zA-Z][a-zA-Z0-9]*)\s*:\s*(INTEGER|REAL|CHAR|BOOLEAN)");
                    if (m.Success)
                    {
                        declaracion.Hijos.Add(new NodoArbol { Texto = "Identificador\n" });
                        declaracion.Hijos.Add(new NodoArbol { Texto = m.Groups[1].Value });
                        declaracion.Hijos.Add(new NodoArbol { Texto = ":" });
                        declaracion.Hijos.Add(new NodoArbol { Texto = "Tipo\n" });
                        declaracion.Hijos.Add(new NodoArbol { Texto = m.Groups[2].Value });
                        declaracion.Hijos.Add(new NodoArbol { Texto = ";" });
                    }
                    declaraciones.Hijos.Add(declaracion);
                }
                else if (linea.StartsWith("BEGIN"))
                {
                    enDeclaraciones = false;
                    enSentencias = true;
                    bloque.Hijos.Add(declaraciones);
                    bloque.Hijos.Add(new NodoArbol { Texto = "BEGIN" });
                    bloque.Hijos.Add(sentencias);
                }
                else if (enSentencias && linea.Contains(":=") && !linea.StartsWith("FOR"))
                {
                    var sentencia = new NodoArbol { Texto = "Sentencia" };
                    string[] partes = linea.Split(new[] { ":=" }, StringSplitOptions.None);

                    sentencia.Hijos.Add(new NodoArbol { Texto = "Identificador\n" });
                    sentencia.Hijos.Add(new NodoArbol { Texto = partes[0].Trim() });
                    sentencia.Hijos.Add(new NodoArbol { Texto = ":=" });

                    string rhs = partes[1].Trim(';', ' ');
                    NodoArbol factor = new NodoArbol { Texto = "Factor" };

                    if (rhs.StartsWith("NOT "))
                    {
                        var operador = new NodoArbol { Texto = "Operador" };
                        operador.Hijos.Add(new NodoArbol { Texto = "NOT" });
                        factor.Hijos.Add(operador);

                        var operando = new NodoArbol { Texto = "Operando" };
                        operando.Hijos.Add(new NodoArbol { Texto = rhs.Substring(4).Trim() });
                        factor.Hijos.Add(operando);
                    }

                    else if (Regex.IsMatch(rhs, @"(.+)([+\-*/])(.+)"))
                    {
                        var opMatch = Regex.Match(rhs, @"(.+?)([+\-*/])(.+)");
                        if (opMatch.Success)
                        {
                            var operando1 = new NodoArbol { Texto = "Operando" };
                            operando1.Hijos.Add(new NodoArbol { Texto = opMatch.Groups[1].Value.Trim() });
                            factor.Hijos.Add(operando1);

                            var operador = new NodoArbol { Texto = "Operador" };
                            operador.Hijos.Add(new NodoArbol { Texto = opMatch.Groups[2].Value.Trim() });
                            factor.Hijos.Add(operador);

                            var operando2 = new NodoArbol { Texto = "Operando" };
                            operando2.Hijos.Add(new NodoArbol { Texto = opMatch.Groups[3].Value.Trim() });
                            factor.Hijos.Add(operando2);
                        }
                        else
                        {
                            var valor = new NodoArbol { Texto = "Valor" };
                            valor.Hijos.Add(new NodoArbol { Texto = rhs });
                            factor.Hijos.Add(valor);
                        }
                    }


                    sentencia.Hijos.Add(factor);
                    sentencia.Hijos.Add(new NodoArbol { Texto = ";" });
                    pilaBloques.Peek().Hijos.Add(sentencia);
                }
                else if (linea.StartsWith("IF"))
                {
                    NodoArbol ifNode = new NodoArbol { Texto = "Condicional IF" };

                    // Extraer la condición sin IF y THEN
                    var condicionNode = new NodoArbol { Texto = "Condición" };
                    string condicion = linea.Replace("IF", "").Replace("THEN", "").Trim();

                    var match = Regex.Match(condicion, @"(.+?)\s*(=|#|<|>|<=|>=|<>)\s*(.+)");
                    if (match.Success)
                    {
                        var operando1 = new NodoArbol { Texto = "Operando" };
                        operando1.Hijos.Add(new NodoArbol { Texto = match.Groups[1].Value.Trim() });

                        var operador = new NodoArbol { Texto = "Operador" };
                        operador.Hijos.Add(new NodoArbol { Texto = match.Groups[2].Value.Trim() });

                        var operando2 = new NodoArbol { Texto = "Operando" };
                        operando2.Hijos.Add(new NodoArbol { Texto = match.Groups[3].Value.Trim() });

                        condicionNode.Hijos.Add(operando1);
                        condicionNode.Hijos.Add(operador);
                        condicionNode.Hijos.Add(operando2);
                    }
                    else
                    {
                        condicionNode.Hijos.Add(new NodoArbol { Texto = condicion });
                    }

                    ifNode.Hijos.Add(condicionNode);
                    pilaBloques.Peek().Hijos.Add(ifNode);
                    pilaBloques.Push(ifNode);
                }

                else if (linea.StartsWith("THEN"))
                {
                    NodoArbol thenNode = new NodoArbol { Texto = "THEN" };
                    pilaBloques.Peek().Hijos.Add(thenNode);
                    pilaBloques.Push(thenNode);
                }
                else if (linea.StartsWith("ELSE"))
                {
                    if (pilaBloques.Count > 1) pilaBloques.Pop();
                    NodoArbol elseNode = new NodoArbol { Texto = "ELSE" };
                    pilaBloques.Peek().Hijos.Add(elseNode);
                    pilaBloques.Push(elseNode);
                }
                else if (linea.StartsWith("END"))
                {
                    if (pilaBloques.Count > 1) pilaBloques.Pop();
                    NodoArbol endNode = new NodoArbol { Texto = "END" };
                    string id = linea.Replace("END", "").Replace(".", "").Trim();
                    if (!string.IsNullOrEmpty(id))
                    {
                        endNode.Hijos.Add(new NodoArbol { Texto = "Identificador\n" });
                        endNode.Hijos.Add(new NodoArbol { Texto = id });
                    }
                    if (linea.Contains(".")) endNode.Hijos.Add(new NodoArbol { Texto = "." });
                    bloque.Hijos.Add(endNode);
                }
                else if (linea.StartsWith("WHILE"))
                {
                    NodoArbol whileNode = new NodoArbol { Texto = "Ciclo WHILE" };

                    var condicion = linea.Replace("WHILE", "").Replace("DO", "").Trim();
                    NodoArbol condicionNode = new NodoArbol { Texto = "Condición" };

                    var match = Regex.Match(condicion, @"(.+?)\s*(=|#|<|>|<=|>=|<>)\s*(.+)");
                    if (match.Success)
                    {
                        var operando1 = new NodoArbol { Texto = "Operando" };
                        operando1.Hijos.Add(new NodoArbol { Texto = match.Groups[1].Value.Trim() });

                        var operador = new NodoArbol { Texto = "Operador" };
                        operador.Hijos.Add(new NodoArbol { Texto = match.Groups[2].Value.Trim() });

                        var operando2 = new NodoArbol { Texto = "Operando" };
                        operando2.Hijos.Add(new NodoArbol { Texto = match.Groups[3].Value.Trim() });

                        condicionNode.Hijos.Add(operando1);
                        condicionNode.Hijos.Add(operador);
                        condicionNode.Hijos.Add(operando2);
                    }
                    else
                    {
                        condicionNode.Hijos.Add(new NodoArbol { Texto = condicion });
                    }

                    whileNode.Hijos.Add(condicionNode);
                    pilaBloques.Peek().Hijos.Add(whileNode);
                    pilaBloques.Push(whileNode);
                }

                else if (linea.StartsWith("DO"))
                {
                    NodoArbol doNode = new NodoArbol { Texto = "DO" };
                    pilaBloques.Peek().Hijos.Add(doNode);
                    pilaBloques.Push(doNode);
                }
                else if (linea.StartsWith("REPEAT"))
                {
                    NodoArbol repeatNode = new NodoArbol { Texto = "Ciclo REPEAT" };
                    repeatNode.Hijos.Add(new NodoArbol { Texto = "REPEAT" });
                    pilaBloques.Peek().Hijos.Add(repeatNode);
                    pilaBloques.Push(repeatNode);
                }
                else if (linea.StartsWith("UNTIL"))
                {
                    NodoArbol untilNode = new NodoArbol { Texto = "UNTIL" };

                    var condicion = linea.Replace("UNTIL", "").Replace(";", "").Trim();
                    NodoArbol condicionNode = new NodoArbol { Texto = "Condición" };

                    var match = Regex.Match(condicion, @"(.+?)\s*(=|#|<|>|<=|>=|<>)\s*(.+)");
                    if (match.Success)
                    {
                        var operando1 = new NodoArbol { Texto = "Operando" };
                        operando1.Hijos.Add(new NodoArbol { Texto = match.Groups[1].Value.Trim() });

                        var operador = new NodoArbol { Texto = "Operador" };
                        operador.Hijos.Add(new NodoArbol { Texto = match.Groups[2].Value.Trim() });

                        var operando2 = new NodoArbol { Texto = "Operando" };
                        operando2.Hijos.Add(new NodoArbol { Texto = match.Groups[3].Value.Trim() });

                        condicionNode.Hijos.Add(operando1);
                        condicionNode.Hijos.Add(operador);
                        condicionNode.Hijos.Add(operando2);
                    }
                    else
                    {
                        condicionNode.Hijos.Add(new NodoArbol { Texto = condicion });
                    }

                    untilNode.Hijos.Add(condicionNode);
                    pilaBloques.Peek().Hijos.Add(untilNode);
                    pilaBloques.Pop(); // cerramos REPEAT
                }

                else if (linea.StartsWith("FOR"))
                {
                    NodoArbol forNode = new NodoArbol { Texto = "Ciclo FOR" };

                    // Ejemplo: FOR i := 1 TO 10 DO
                    var match = Regex.Match(linea, @"FOR\s+([a-zA-Z][a-zA-Z0-9]*)\s*:=\s*(\d+)\s+TO\s+(\d+)\s+DO", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var controlNode = new NodoArbol { Texto = "Control" };

                        controlNode.Hijos.Add(new NodoArbol { Texto = "Variable" });
                        controlNode.Hijos.Add(new NodoArbol { Texto = match.Groups[1].Value });

                        controlNode.Hijos.Add(new NodoArbol { Texto = ":=" });

                        controlNode.Hijos.Add(new NodoArbol { Texto = "Inicio" });
                        controlNode.Hijos.Add(new NodoArbol { Texto = match.Groups[2].Value });

                        controlNode.Hijos.Add(new NodoArbol { Texto = "TO" });

                        controlNode.Hijos.Add(new NodoArbol { Texto = "Fin" });
                        controlNode.Hijos.Add(new NodoArbol { Texto = match.Groups[3].Value });

                        forNode.Hijos.Add(controlNode);
                    }
                    else
                    {
                        forNode.Hijos.Add(new NodoArbol { Texto = "Control\n" + linea });
                    }

                    pilaBloques.Peek().Hijos.Add(forNode);
                    pilaBloques.Push(forNode);
                }

            }

            return raiz;
        }

        private void MostrarArbol(NodoArbol raiz)
        {
            tvArbol.Nodes.Clear();
            TreeNode nodoRaiz = CrearNodo(raiz);
            tvArbol.Nodes.Add(nodoRaiz);
            tvArbol.ExpandAll();
        }

        private TreeNode CrearNodo(NodoArbol nodo)
        {
            TreeNode treeNode = new TreeNode(nodo.Texto);
            foreach (var hijo in nodo.Hijos)
            {
                treeNode.Nodes.Add(CrearNodo(hijo));
            }
            return treeNode;
        }

        private void dgvTablaSimbolos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }

    public class NodoArbol
    {
        public string Texto { get; set; }
        public List<NodoArbol> Hijos { get; set; } = new List<NodoArbol>();
    }
}
