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
            //Asignación del dgv para mostrar tabla de símbolos
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

        //clase para manejar las variables de la tabla de símbolos
        public class VariableSimbolo
        {
            public string Token { get; set; }
            public string Nombre { get; set; }
            public string Tipo { get; set; } = "";
            public string Valor { get; set; } = "";
            public int LineaAsignacion { get; set; } = -1;
            public List<int> Referencias { get; set; } = new List<int>();
        }

        // Diccionario que actúa como la tabla de símbolos del compilador.
        // Aquí se guardan las variables declaradas junto con su tipo, valor, línea de asignación y referencias.
        private Dictionary<string, VariableSimbolo> tablaSimbolos = new Dictionary<string, VariableSimbolo>();

        // Este diccionario lleva un conteo por cada categoría léxica
        // Se usa para generar los tokens con un identificador de secuencia (único para cada tokend)
        private Dictionary<int, int> contadorPorCategoria = new Dictionary<int, int>();

        // Conjunto (HashSet) que contiene todas las palabras reservadas del lenguaje Modula-2.
        // Se usa para reconocer estas palabras durante el análisis léxico y validar que estén bien escritas (en mayúsculas).
        private HashSet<string> palabrasReservadas = new HashSet<string>
        {
            "AND", "ARRAY", "BEGIN", "BOOLEAN", "BY", "CASE", "CHAR", "CONST", "DEFINITION",
            "DIV", "DO", "ELSE", "ELSIF", "END", "EXIT", "EXPORT", "FALSE", "FOR", "FROM",
            "IF", "IMPORT", "IN", "INTEGER", "LOOP", "MOD", "MODULE", "NOT", "OF", "OR",
            "POINTER", "PROCEDURE", "QUALIFIED", "REAL", "RECORD", "REPEAT", "RETURN", "SET",
            "THEN", "TO", "TRUE", "TYPE", "UNTIL", "VAR", "WHILE", "WITH"
        };

        private void btnAnalisisLexico_Click(object sender, EventArgs e)
        {
            //Llamar método
            AnalizarCodigo();
        }

        // Método principal que realiza el análisis del código fuente ingresado.
        private void AnalizarCodigo()
        {
            // Limpia la interfaz de cualquier análisis anterior.
            dgvTablaSimbolos.Rows.Clear();     // Limpia la tabla de símbolos.
            lbTokens.Items.Clear();            // Limpia la lista de tokens.
            lbControl.Items.Clear();           // Limpia la lista de control de errores.
            tablaSimbolos.Clear();             // Limpia la estructura que guarda las variables del programa.
            contadorPorCategoria.Clear();      // Reinicia el contador para los tokens por categoría.
            List<string> erroresLexicos = new List<string>();  // Lista temporal para guardar los errores léxicos.

            // Separa el texto del editor por líneas para analizarlas una por una.
            string[] lineas = txtBloqueCodigo.Text.Split('\n');
            int numLinea = 1;

            // ─────── ETAPA 1: Construcción de la Tabla de Símbolos ───────
            foreach (string raw in lineas)
            {
                string linea = raw.Trim();

                // Busca declaraciones de variables con sus tipos (por ejemplo: a : INTEGER)
                var declaraciones = Regex.Matches(linea, @"([a-zA-Z][a-zA-Z0-9]*)\s*:\s*(INTEGER|REAL|BOOLEAN|CHAR)");
                foreach (Match match in declaraciones)
                {
                    string nombre = match.Groups[1].Value;
                    string tipo = match.Groups[2].Value;
                    RegistrarVariable(nombre, tipo, numLinea); // Registra la variable en la tabla de símbolos.
                }

                // Busca asignaciones (por ejemplo: a := 5;)
                var asignacion = Regex.Match(linea, @"^([a-zA-Z][a-zA-Z0-9]*)\s*:=\s*(.+);$");
                if (asignacion.Success)
                {
                    string lhs = asignacion.Groups[1].Value; // Variable a la izquierda
                    string rhs = asignacion.Groups[2].Value; // Valor a la derecha
                    AsignarValor(lhs, rhs, numLinea); // Registra el valor asignado

                    // Busca posibles referencias a otras variables en la parte derecha.
                    var posibles = Regex.Split(rhs, @"[^a-zA-Z0-9]+");
                    foreach (var refNombre in posibles)
                    {
                        if (tablaSimbolos.ContainsKey(refNombre) && refNombre != lhs)
                        {
                            if (!tablaSimbolos[refNombre].Referencias.Contains(numLinea))
                                tablaSimbolos[refNombre].Referencias.Add(numLinea); // Marca la línea donde se usó esa variable.
                        }
                    }
                }

                // Revisa cada variable en la tabla para ver si aparece en la línea actual.
                foreach (var simbolo in tablaSimbolos.Values)
                {
                    var regex = new Regex(@"\b" + Regex.Escape(simbolo.Nombre) + @"\b");
                    if (regex.IsMatch(linea) && !simbolo.Referencias.Contains(numLinea))
                    {
                        simbolo.Referencias.Add(numLinea); // Agrega la línea como referencia si no lo estaba ya.
                    }
                }

                numLinea++;
            }

            MostrarTablaSimbolos(); // Muestra la tabla con variables registradas en la interfaz.

            // ─────── ETAPA 2: Análisis Léxico y Detección de Errores ───────
            contadorPorCategoria.Clear(); // Reinicia contadores para asignar tokens.
            numLinea = 1;

            foreach (string raw in lineas)
            {
                string linea = raw.Trim();

                // Separa los tokens de cada línea usando expresiones regulares.
                string[] tokens = Regex.Split(linea, @"(\s+|:=|\.{2}|>=|<=|<>|=|#|<|>|\+|\-|\*|/|:|;|,|\.|\(|\)|\[|\]|\{|\})");

                foreach (string rawToken in tokens)
                {
                    string token = rawToken.Trim();
                    if (string.IsNullOrEmpty(token)) continue;

                    string upperToken = token.ToUpper();

                    // Identifica si es palabra reservada
                    if (palabrasReservadas.Contains(upperToken))
                        RegistrarToken(token, 5);

                    // Operadores lógicos
                    else if (upperToken == "AND" || upperToken == "OR" || upperToken == "NOT")
                        RegistrarToken(token, 40);

                    // Identificador (posible variable)
                    else if (Regex.IsMatch(token, @"^[a-zA-Z][a-zA-Z0-9]*$"))
                    {
                        // Verifica si se parece a una palabra reservada pero está mal escrita
                        if (palabrasReservadas.Any(res => res.StartsWith(token) || token.StartsWith(res)) && !palabrasReservadas.Contains(token.ToUpper()))
                        {
                            erroresLexicos.Add($"[Línea {numLinea}] Error léxico: Token inválido '{token}' (posible palabra reservada mal escrita)");
                        }
                        else
                        {
                            RegistrarToken(token, 10);
                        }
                    }

                    // Constantes y otros símbolos
                    else if (Regex.IsMatch(token, @"^\d+$")) RegistrarToken(token, 15); // Número entero
                    else if (Regex.IsMatch(token, @"^\d+\.\d+$")) RegistrarToken(token, 15); // Número real
                    else if (Regex.IsMatch(token, @"^'.'$")) RegistrarToken(token, 20); // Carácter
                    else if (Regex.IsMatch(token, "^\".*\"$")) RegistrarToken(token, 25); // Cadena
                    else if ("+-*/".Contains(token)) RegistrarToken(token, 30); // Operador aritmético
                    else if (new[] { "=", "#", "<", ">", "<=", ">=", "<>" }.Contains(token)) RegistrarToken(token, 35); // Relacional
                    else if (token == "..") RegistrarToken(token, 45); // Rango
                    else if (token == ":=") RegistrarToken(token, 50); // Asignación
                    else if (token == ":") RegistrarToken(token, 55); // Separador tipo
                    else if (token == ";" || token == ",") RegistrarToken(token, 60); // Separadores
                    else if (token == ".") RegistrarToken(token, 65); // Punto final
                    else if (new[] { "{", "}", "(", ")", "[", "]" }.Contains(token)) RegistrarToken(token, 70); // Agrupadores
                    else
                    {
                        // Si no coincide con ninguna categoría, es un error léxico.
                        erroresLexicos.Add($"[Línea {numLinea}] Error léxico: Token no reconocido '{token}'");
                    }
                }

                numLinea++;
            }

            // ─────── ETAPA FINAL: Mostrar errores o continuar con el análisis sintáctico ───────

            if (erroresLexicos.Count > 0)
            {
                foreach (string error in erroresLexicos)
                    lbControl.Items.Add(error);

                lbControl.Items.Add(" Árbol sintáctico no generado debido a errores léxicos.");
            }
            else
            {
                lbControl.Items.Add(" Análisis léxico completado sin errores.");
                lbControl.Items.Add($"Líneas analizadas: {numLinea - 1}");

                // Solo si no hay errores léxicos, se genera el árbol sintáctico
                NodoArbol arbol = ConstruirArbol(lineas);
                MostrarArbol(arbol);
            }
        }



        private void RegistrarVariable(string nombre, string tipo, int linea)
        {
            // Evita registrar nombres que coincidan con palabras reservadas o con el nombre especial "SUMA".
            if (palabrasReservadas.Contains(nombre.ToUpper()) || nombre.ToUpper() == "SUMA")
                return;

            // Verifica si la variable ya está registrada en la tabla de símbolos.
            if (!tablaSimbolos.ContainsKey(nombre))
            {
                int cod = 10; // Código de categoría léxica para identificadores (variables).

                // Inicializa el contador para esta categoría si es la primera vez.
                if (!contadorPorCategoria.ContainsKey(cod))
                    contadorPorCategoria[cod] = 1;
                else
                    contadorPorCategoria[cod]++;

                // Crea un token con formato: ### + código (ej. 00110, 00210, etc.)
                string token = contadorPorCategoria[cod].ToString("D3") + cod.ToString("D2");

                // Registra la variable en la tabla con todos sus atributos.
                tablaSimbolos[nombre] = new VariableSimbolo
                {
                    Token = token,
                    Nombre = nombre,
                    Tipo = tipo,
                    LineaAsignacion = linea,                    // Línea donde fue declarada.
                    Referencias = new List<int> { linea }       // Primera referencia = declaración.
                };
            }
        }

        private void AsignarValor(string nombre, string valor, int linea)
        {
            // Verifica si la variable ya fue declarada y está registrada en la tabla de símbolos.
            if (tablaSimbolos.ContainsKey(nombre))
            {
                // Actualiza el valor de la variable.
                tablaSimbolos[nombre].Valor = valor;

                // Si esta línea no ha sido registrada como referencia aún, se añade.
                if (!tablaSimbolos[nombre].Referencias.Contains(linea))
                    tablaSimbolos[nombre].Referencias.Add(linea);
            }
        }

        private void RegistrarToken(string lexema, int categoria)
        {
            // Inicializa el número secuencial para esta categoría
            int secuencia = 1;

            // Si ya existe esta categoría en el contador, incrementa su secuencia
            if (contadorPorCategoria.ContainsKey(categoria))
                secuencia = ++contadorPorCategoria[categoria];
            else
                contadorPorCategoria[categoria] = secuencia;

            // Crea el token en formato "nnncc" (3 dígitos de secuencia + 2 de categoría)
            string token = secuencia.ToString("D3") + categoria.ToString("D2");

            // Verifica si el token ya fue agregado para evitar duplicados
            bool yaExiste = lbTokens.Items.Cast<string>().Any(item => item.Contains($"{token} {lexema}"));
            if (!yaExiste)
                lbTokens.Items.Add($"{token} {lexema}");
        }

        private void MostrarTablaSimbolos()
        {
            // Limpia cualquier fila anterior
            dgvTablaSimbolos.Rows.Clear();

            // Recorre todas las variables almacenadas en la tabla de símbolos
            foreach (var s in tablaSimbolos.Values)
            {
                // Une las líneas de referencia en una cadena separada por comas
                string referencias = string.Join(",", s.Referencias.OrderBy(x => x));

                // Agrega una nueva fila con toda la información de la variable
                dgvTablaSimbolos.Rows.Add(
                    s.Token,                        // ID del token
                    s.Nombre,                      // Nombre de la variable
                    s.Tipo,                        // Tipo (INTEGER, REAL, etc.)
                    s.Valor,                       // Valor actual asignado
                    s.LineaAsignacion == -1 ? "" : s.LineaAsignacion.ToString(), // Línea donde fue asignada
                    referencias                    // Líneas donde es referenciada
                );
            }
        }

        // Árbol sintáctico
        private NodoArbol ConstruirArbol(string[] lineas)
        {
            // Nodo raíz del programa
            NodoArbol raiz = new NodoArbol { Texto = "Programa" };
            // Nodos principales del bloque del programa
            NodoArbol bloque = new NodoArbol { Texto = "Bloque" };
            NodoArbol declaraciones = new NodoArbol { Texto = "Declaraciones" };
            NodoArbol sentencias = new NodoArbol { Texto = "Sentencias" };

            bool enDeclaraciones = false;
            bool enSentencias = false;

            // Pila para manejar estructuras anidadas como IF, WHILE, FOR, etc.
            Stack<NodoArbol> pilaBloques = new Stack<NodoArbol>();
            pilaBloques.Push(sentencias); // Por defecto todo se anida en sentencias

            foreach (string raw in lineas)
            {
                string linea = raw.Trim();
                if (string.IsNullOrWhiteSpace(linea)) continue;

                // Detectar inicio del módulo
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
                // Detectar sección VAR (declaraciones)
                else if (linea.StartsWith("VAR"))
                {
                    enDeclaraciones = true;
                    declaraciones.Hijos.Add(new NodoArbol { Texto = "VAR" });
                }
                // Declaraciones de variables
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
                // Inicio de bloque de sentencias
                else if (linea.StartsWith("BEGIN"))
                {
                    enDeclaraciones = false;
                    enSentencias = true;
                    bloque.Hijos.Add(declaraciones);
                    bloque.Hijos.Add(new NodoArbol { Texto = "BEGIN" });
                    bloque.Hijos.Add(sentencias);
                }
                // Sentencia de asignación (ej. resultado := a + b;)
                else if (enSentencias && linea.Contains(":=") && !linea.StartsWith("FOR"))
                {
                    var sentencia = new NodoArbol { Texto = "Sentencia" };
                    string[] partes = linea.Split(new[] { ":=" }, StringSplitOptions.None);

                    // Parte izquierda (variable)
                    sentencia.Hijos.Add(new NodoArbol { Texto = "Identificador\n" });
                    sentencia.Hijos.Add(new NodoArbol { Texto = partes[0].Trim() });
                    sentencia.Hijos.Add(new NodoArbol { Texto = ":=" });

                    // Parte derecha (expresión o valor)
                    string rhs = partes[1].Trim(';', ' ');
                    NodoArbol factor = new NodoArbol { Texto = "Factor" };

                    // Caso: expresión con NOT
                    if (rhs.StartsWith("NOT "))
                    {
                        var operador = new NodoArbol { Texto = "Operador" };
                        operador.Hijos.Add(new NodoArbol { Texto = "NOT" });
                        factor.Hijos.Add(operador);

                        var operando = new NodoArbol { Texto = "Operando" };
                        operando.Hijos.Add(new NodoArbol { Texto = rhs.Substring(4).Trim() });
                        factor.Hijos.Add(operando);
                    }
                    // Caso: expresión aritmética
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
                // Sentencia IF con condición
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
                // uso de then
                else if (linea.StartsWith("THEN"))
                {
                    NodoArbol thenNode = new NodoArbol { Texto = "THEN" };
                    pilaBloques.Peek().Hijos.Add(thenNode);
                    pilaBloques.Push(thenNode);
                }
                // uso de else
                else if (linea.StartsWith("ELSE"))
                {
                    if (pilaBloques.Count > 1) pilaBloques.Pop();
                    NodoArbol elseNode = new NodoArbol { Texto = "ELSE" };
                    pilaBloques.Peek().Hijos.Add(elseNode);
                    pilaBloques.Push(elseNode);
                }

                // END (finaliza estructuras como IF o WHILE)
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
                // WHILE
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
                // DO
                else if (linea.StartsWith("DO"))
                {
                    NodoArbol doNode = new NodoArbol { Texto = "DO" };
                    pilaBloques.Peek().Hijos.Add(doNode);
                    pilaBloques.Push(doNode);
                }
                // REPEAT
                else if (linea.StartsWith("REPEAT"))
                {
                    NodoArbol repeatNode = new NodoArbol { Texto = "Ciclo REPEAT" };
                    repeatNode.Hijos.Add(new NodoArbol { Texto = "REPEAT" });
                    pilaBloques.Peek().Hijos.Add(repeatNode);
                    pilaBloques.Push(repeatNode); // Cierra el bloque REPEAT
                }
                // UNTIL
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
                // FOR
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

            return raiz; // Devuelve el árbol sintáctico construido
        }

        private void MostrarArbol(NodoArbol raiz)
        {
            // Limpia todos los nodos previos del control TreeView.
            tvArbol.Nodes.Clear();
            // Crea el nodo principal del TreeView a partir del nodo raíz del árbol sintáctico.
            TreeNode nodoRaiz = CrearNodo(raiz);
            // Agrega el nodo creado al TreeView.
            tvArbol.Nodes.Add(nodoRaiz);
            // Expande todos los nodos del árbol para mostrar su contenido completo.
            tvArbol.ExpandAll();
        }

        private TreeNode CrearNodo(NodoArbol nodo)
        {
            // Crea un TreeNode visual con el texto del nodo lógico.
            TreeNode treeNode = new TreeNode(nodo.Texto);
            // Para cada hijo del nodo lógico, se llama recursivamente a CrearNodo
            // y se agrega al TreeNode visual como hijo.
            foreach (var hijo in nodo.Hijos)
            {
                treeNode.Nodes.Add(CrearNodo(hijo));
                // Devuelve el nodo visual generado.
            }
            return treeNode;
        }

        private void dgvTablaSimbolos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Limpiar TextBox del código
            txtBloqueCodigo.Clear();

            // Limpiar lista de tokens
            lbTokens.Items.Clear();

            // Limpiar lista de errores y mensajes
            lbControl.Items.Clear();

            // Limpiar árbol sintáctico
            tvArbol.Nodes.Clear();

            // Limpiar tabla de símbolos
            dgvTablaSimbolos.Rows.Clear();

            // Limpiar estructuras internas
            erroresLexicos.Clear();
            tablaSimbolos.Clear();
            contadorPorCategoria.Clear();
        }
    }

    public class NodoArbol
    {
        public string Texto { get; set; }
        public List<NodoArbol> Hijos { get; set; } = new List<NodoArbol>();
    }
}
