using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using ConsoleTables;
using System.Linq;
using System.Collections.Generic;

public class ActThirteen
{
    public ArrayList executeProgram(string path, string[] wordsToFind, bool stoplistEnabled, string url)
    {
        //INICIA EL CRONOMETRO
        Stopwatch watch = Stopwatch.StartNew();
        if (stoplistEnabled)
        {
            Console.WriteLine("Actividad 13 con stoplist en proceso... ");
        }
        else
        {
            Console.WriteLine("Actividad 13 sin stoplist en proceso... ");
        }


        //OBTIENE LAS RUTAS DE LOS ARCHIVOS HTML
        string[] filePaths = Directory.GetFiles(path + "\\results\\act3\\files");

        //OBTIENE LAS PALABRAS DE LA STOPLIST
        string[] stopList = File.ReadAllLines(path + "\\utils\\stoplist.html");

        //SE CREA UN ARCHIVO TXT DE SALIDA DE DATOS
        Directory.CreateDirectory(path + "\\results\\act13");

        //TABLA PARA GUARDAR LOS DATOS DE MANERA ORDENADA
        var dataTable = new ConsoleTable("Palabra", "Existencia", "Posting");
        var dataTablePostings = new ConsoleTable("Palabra", "ID Documento", "Peso");
        var dataTableRelations = new ConsoleTable("ID", "Documento");


        //GUARDA RELACION ID - DOCUMENTO HTML
        Dictionary<int, string> relationDocs = new Dictionary<int, string>();

        //GUARDA TODAS LAS PALABRAS Y SU FRECUENCIA
        List<Word> wordsList = new List<Word>();

        //GUARDA LAS PALABRAS EN UN DICCIONARIO JUNTO CON SUS FILEPATHS
        Dictionary<string, Dictionary<string, int>> wordsSearched = new Dictionary<string, Dictionary<string, int>>();

        //ENLISTA LA FRECUENCIA DE LAS PALABRAS POR ARCHIVO
        Dictionary<string, Word> freqDict;
        using (var progress = new ProgressBar())
        {
            progress.setTask("Calculando frecuencia de palabras");
            int count = 0;
            int countDocs = 1;

            foreach (string wordToFind in wordsToFind)
            {
                List<string> docSearch = new List<string>();
                foreach (string filepath in filePaths)
                {
                    freqDict = new Dictionary<string, Word>();
                    string[] words = File.ReadAllLines(filepath);
                    //GUARDA LOS DOCUMENTOS QUE CONTENGAN LA PALABRA BUSCADA

                    foreach (string word in words)
                    {
                        if (stoplistEnabled)
                        {
                            foreach (string stopWord in stopList)
                            {
                                if (word.ToLower() != stopWord.ToLower())
                                {
                                    if (!freqDict.ContainsKey(word))
                                    {
                                        freqDict.Add(word.ToLower(), new Word(word.ToLower()/*, filepath*/));
                                    }
                                    freqDict[word.ToLower()].freq++;
                                    //CONSIGUE EL FILE NAME DEL PATH
                                    string fileName = url + (string)Path.GetFileName(filepath);
                                    //REVISA SI EXISTE YA EL ARCHIVO EN EL DICCIONARIO
                                    if (!freqDict[word.ToLower()].KeyFileRepetitions.ContainsKey(fileName))
                                    {
                                        //AGREGA EL NOMBRE DEL ARCHIVO AL DICCIONARIO
                                        freqDict[word.ToLower()].KeyFileRepetitions.Add(fileName, 0);
                                    }
                                    freqDict[word.ToLower()].KeyFileRepetitions[fileName]++;
                                }
                            }
                        }
                        else
                        {
                            if (!freqDict.ContainsKey(word.ToLower()))
                            {
                                freqDict.Add(word.ToLower(), new Word(word.ToLower()/*, filepath*/));
                            }
                            freqDict[word.ToLower()].freq++;
                            //CONSIGUE EL FILE NAME DEL PATH
                            string fileName = url + (string)Path.GetFileName(filepath);
                            //REVISA SI EXISTE YA EL ARCHIVO EN EL DICCIONARIO
                            if (!freqDict[word.ToLower()].KeyFileRepetitions.ContainsKey(fileName))
                            {
                                //AGREGA EL NOMBRE DEL ARCHIVO AL DICCIONARIO
                                freqDict[word.ToLower()].KeyFileRepetitions.Add(fileName, 0);
                            }
                            freqDict[word.ToLower()].KeyFileRepetitions[fileName]++;
                        }

                        //ACTIVIDAD 13
                        if (wordToFind == word.ToLower())
                        {
                            docSearch.Add(url + (string)Path.GetFileName(filepath));
                        }
                    }
                    foreach (KeyValuePair<string, Word> word in freqDict)
                    {
                        if (word.Value.freq >= 3)
                        {
                            wordsList.Add(word.Value);
                        }
                    }

                    //PROGRESO
                    count++;
                    relationDocs.Add(countDocs, url + (string)Path.GetFileName(filepath));
                    countDocs++;
                    progress.Report((double)count / filePaths.Length);
                }
                Dictionary<string, int> repeatedPaths = new Dictionary<string, int>();
                foreach (string filepath in docSearch)
                {
                    if (!repeatedPaths.ContainsKey(filepath))
                    {
                        repeatedPaths.Add(filepath, 1);
                    }
                    repeatedPaths[filepath]++;
                }
                wordsSearched.Add(wordToFind, repeatedPaths);
            }

            //IMPRIME EL ULTIMO LOG DE PROGRESO
            Console.WriteLine("\n[##########]  100%  | Calculando frecuencia de palabras");
        }

        //ENLISTA LA EXISTENCIA DE LAS PALABRAS POR ARCHIVO
        SortedDictionary<string, Word> exDict = new SortedDictionary<string, Word>();
        using (var progress = new ProgressBar())
        {
            progress.setTask("Calculando existencia de palabras");
            int count = 0;
            foreach (Word wordObj in wordsList)
            {
                if (!exDict.ContainsKey(wordObj.content))
                {
                    exDict.Add(wordObj.content, new Word(wordObj.content, wordObj.freq));
                }
                exDict[wordObj.content].freq += wordObj.freq;
                exDict[wordObj.content].freqFile++;
                foreach (KeyValuePair<string, int> temp in wordObj.KeyFileRepetitions)
                {
                    exDict[wordObj.content].references.Add(temp);
                }

                //PROGRESO
                count++;
                progress.Report((double)count / wordsList.Count);
            }

            //IMPRIME EL ULTIMO LOG DE PROGRESO        
            Console.WriteLine("\n[##########]  100%  | Calculando existencia de palabras");
        }

        //HASHTABLE PARA GUARDAR LOS DATOS
        Hashtable hTable = new Hashtable(exDict);

        //ARRAY PARA GUARDAR EL RESULTADO
        int freqPosting = 0;
        foreach (DictionaryEntry wordObj in hTable)
        {
            dataTable.AddRow(wordObj.Key,
                            (wordObj.Value as Word).freqFile,
                             freqPosting);
            freqPosting += (wordObj.Value as Word).freqFile;

            foreach (KeyValuePair<string, int> repetition in (wordObj.Value as Word).references)
            {
                dataTablePostings.AddRow((wordObj.Value as Word).content,
                                          relationDocs.FirstOrDefault(x => x.Value == repetition.Key).Key,
                                          ((wordObj.Value as Word).freq * 100) / (wordObj.Value as Word).freqFile);
            }
        }

        foreach (KeyValuePair<int, string> document in relationDocs)
        {
            dataTableRelations.AddRow(document.Key, document.Value);
        }


        foreach (KeyValuePair<string, Dictionary<string, int>> word in wordsSearched)
        {
            ArrayList test = new ArrayList();
            var dataTableDocSearch = new ConsoleTable("TOP", "Documento", "Frecuencia");
            var listKV = word.Value.ToList();
            listKV.Sort((first, next) => {
                return next.Value.CompareTo(first.Value);
            });
            if (listKV.Count > 10)
            {
                listKV.RemoveRange(10, listKV.Count - 10);
            }

            int counter = 1;
            foreach (KeyValuePair<string, int> filepath in listKV)
            {
                dataTableDocSearch.AddRow(/*relationDocs.FirstOrDefault(x => x.Value == filepath.Key).Key*/ counter, filepath.Key, filepath.Value);
                test.Add(filepath);
                counter++;
            }
            string title = "Palabra buscada: " + word.Key + "\n--------------------------\n";
            //File.WriteAllText(path + "\\results\\act13\\searchFor" + FirstCharToUpper(word.Key) + ".html", title + dataTableDocSearch.ToMinimalString());
            return test;
        }

        //TERMINA EL CRONOMETRO
        watch.Stop();

        return new ArrayList();

        //ESCRIBE TODAS LAS PALABRAS EN UN ARCHIVO CONSOLIDADO
        File.WriteAllText(path + "\\results\\act13\\posting.html", dataTablePostings.ToMinimalString());
        File.WriteAllText(path + "\\results\\act13\\file.html", dataTable.ToMinimalString());
        File.WriteAllText(path + "\\results\\act13\\relation.html", dataTableRelations.ToMinimalString());

        File.WriteAllText(path + "\\results\\act13\\results.txt", "\nTiempo total en ejecutar el programa: " + watch.Elapsed);
        Console.WriteLine("Actividad 13 completada exitosamente, Noice\n");
    }

    public static string FirstCharToUpper(string s)
    {
        // Check for empty string.  
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        // Return char and concat substring.  
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}

