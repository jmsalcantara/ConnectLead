using ConnectLead.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace ConnectLead.Controllers
{
    public class FavoritosController : Controller
    {
        // GET: Favoritos
        public ActionResult Index()
        {
            string prop = "", repo = "", check = "";
            string caminho = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\favoritos.txt";
            RestClient client = new RestClient(ConfigurationManager.AppSettings["UrlBase"]);
            List<Repositorio.RootObject> repositorios = new List<Repositorio.RootObject>();

            if (System.IO.File.Exists(caminho))
            {
                using (StreamReader sr = new StreamReader(caminho))
                {
                    String linha;
                    // Lê linha por linha até o final do arquivo
                    while ((linha = sr.ReadLine()) != null)
                    {
                        //Console.WriteLine(linha);
                        if (linha != null)
                        {
                            string[] dados = linha.Split(',');
                            prop = dados[0];
                            repo = dados[1];
                            check = dados[2];
                        }

                        String urlRepositorio = "repos/" + prop + "/" + repo;

                        RestRequest requisicao = new RestRequest(
                            urlRepositorio,
                            Method.GET);

                        IRestResponse<Repositorio.RootObject> resposta =
                            client.Execute<Repositorio.RootObject>(requisicao);

                       repositorios.Add(resposta.Data);
                    }
                }
            }

            return View(repositorios);
        }
    }
}