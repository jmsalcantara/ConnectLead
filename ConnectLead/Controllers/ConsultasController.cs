using ConnectLead.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ConnectLead.Controllers
{
    public class ConsultasController : Controller
    {
        // GET: Consultas
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string parametro)
        {
            String urlConsulta = "search/repositories?q=" + parametro;

            RestClient client = new RestClient(
               ConfigurationManager.AppSettings["UrlBase"]);

            RestRequest requisicao = new RestRequest(
                urlConsulta,
                Method.GET);

            IRestResponse<List<Consulta.RootObject>> resposta =
                client.Execute<List<Consulta.RootObject>>(requisicao);

            List<Consulta.RootObject> repositorios = resposta.Data;

            return View(repositorios);
        }

    }
}