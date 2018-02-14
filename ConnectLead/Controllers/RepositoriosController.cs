using ConnectLead.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace ConnectLead.Controllers
{
    public class RepositoriosController : Controller
    {
        public ActionResult Index()
        {
            RestClient client = new RestClient(
                ConfigurationManager.AppSettings["UrlBase"]);

            RestRequest requisicao = new RestRequest(
                ConfigurationManager.AppSettings["UrlMeuRepositorio"],
                Method.GET);

            IRestResponse<List<Repositorio.RootObject>> resposta =
                client.Execute<List<Repositorio.RootObject>>(requisicao);

            List<Repositorio.RootObject> repositorios = resposta.Data;

            return View(repositorios);
        }

        public ActionResult DetalhesRepositorio(string nomeProprietario, string nomeRepositorio)
        {
            string prop = "", repo = "", check = "";
            String urlDetahesRepositorio = "repos/" + nomeProprietario + "/" + nomeRepositorio;
            String urlContribuidores = "repos/" + nomeProprietario + "/" + nomeRepositorio + "/contributors";

            RestClient client = new RestClient(
               ConfigurationManager.AppSettings["UrlBase"]);

            RestRequest requisicao = new RestRequest(
                urlDetahesRepositorio,
                Method.GET);

            IRestResponse<Repositorio.RootObject> resposta =
                client.Execute<Repositorio.RootObject>(requisicao);

            RestRequest requisicao2 = new RestRequest(
                urlContribuidores,
                Method.GET);

            IRestResponse<List<Contribuidor>> resposta2 =
                client.Execute<List<Contribuidor>>(requisicao2);

            Repositorio.RootObject repositorios = resposta.Data;
            List<Contribuidor> contribuidores = resposta2.Data;
            ViewBag.Repositorio = repositorios;
            ViewBag.Contribuidores = contribuidores;

            string caminho = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\favoritos.txt";

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
                        if (repositorios.owner.login == prop && repositorios.name == repo && check == "on")
                        {
                            //Session["Check"] = "on";
                            ViewBag.Check = "on";
                        }
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult PesquisaResultado()
        {
            return View();
        }

        static class Constants
        {
            public const int paginaInicial = 0;
        }

        [HttpPost]
        public ActionResult PesquisaResultado(int? paginador, string parametro)
        {
            if (parametro != null)
            {
                Session["param"] = parametro;
                Session["pag"] = Constants.paginaInicial;
            }

            int pag = int.Parse(Session["pag"].ToString());
            if (paginador == 0 && pag > 0)
            {
                Session["pag"] = pag - 1;
            }
            else
            {
                Session["pag"] = pag + 1;
            }
            String urlConsulta = "search/repositories?q=" + Session["param"] + "in:name&page=" + Session["pag"];

            RestClient client = new RestClient(
               ConfigurationManager.AppSettings["UrlBase"]);

            RestRequest requisicao = new RestRequest(
                urlConsulta,
                Method.GET);

            IRestResponse<List<Consulta.RootObject>> resposta =
                 client.Execute<List<Consulta.RootObject>>(requisicao);

            List<Consulta.RootObject> repositorios = resposta.Data;
            int totalRegistros = repositorios[0].total_count;
            int paginas = totalRegistros / 30;
            if ((paginas % 2) != 0 || paginas == 0)
            {
                paginas++;
            }
            ViewBag.TotalPaginas = paginas;
            return View(repositorios[0].items);
        }

        [HttpPost]
        public ActionResult MarcarFavorito(string repositorio, string proprietario, string chk_favorito)
        {
            ViewBag.Repositorio = (Repositorio.RootObject)Session["repositorios"];
            ViewBag.Contribuidores = (IEnumerable<Contribuidor>)Session["contribuidores"];

            string prop = "", repo = "", check = "";
            string caminho = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\favoritos.txt";

            ViewBag.Check = chk_favorito;

            if (chk_favorito != null)
            {
                if (!System.IO.File.Exists(caminho))
                {
                    StreamWriter favoritos = new StreamWriter(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\favoritos.txt");
                    favoritos.WriteLine(proprietario + "," + repositorio + "," + chk_favorito);
                    favoritos.Close();
                }
                else
                {
                    StreamWriter favoritos = new StreamWriter(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.ToString()) + @"\favoritos.txt", true);
                    favoritos.WriteLine(proprietario + "," + repositorio + "," + chk_favorito);
                    favoritos.Close();
                }
            }
            else
            {
                if (System.IO.File.Exists(caminho))
                {
                    using (StreamReader sr = new StreamReader(caminho))
                    {
                        String linha;
                        int cont = -1;
                        // Lê linha por linha até o final do arquivo
                        while ((linha = sr.ReadLine()) != null)
                        {
                            cont++;
                            if (linha != null)
                            {
                                string[] dados = linha.Split(',');
                                prop = dados[0];
                                repo = dados[1];
                                check = dados[2];
                            }
                            if (proprietario == prop && repositorio == repo && chk_favorito == null)
                            {
                                var file = new List<string>(System.IO.File.ReadAllLines(caminho));
                                sr.Close();
                                file.RemoveAt(cont);
                                System.IO.File.WriteAllLines(caminho, file.ToArray());
                                break;
                            }
                        }
                    }
                }
            }
            return RedirectToAction("DetalhesRepositorio", new { nomeProprietario = proprietario, nomeRepositorio = repositorio });
        }
    }
}