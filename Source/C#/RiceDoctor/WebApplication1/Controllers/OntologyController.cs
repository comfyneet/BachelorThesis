using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using static RiceDoctor.OntologyManager.GetType;
using Attribute = RiceDoctor.OntologyManager.Attribute;
using RiceDoctor.SemanticCode;

namespace WebApplication1.Controllers
{
    public class OntologyController : Controller
    {
        private IOntologyManager _manager;

        public OntologyController([FromServices] IOntologyManager manager)
        {
            _manager = manager;
            _manager.GetClass("Thing");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Chào Mừng đến Hệ tư vấn chuẩn đoán ";
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Relation()
        {
            var a = _manager.GetClass("Thing");
            var b = _manager.GetRelations();
            var c = '0';
            ViewBag.allA = b;
            ViewBag.co = c;
            return View("Option1");
        }

        public IActionResult Attribult()
        {
            var a = _manager.GetClass("Thing");
            var attributes = _manager.GetAttributes();
            ViewBag.allA = attributes;
            var c = '1';
            ViewBag.co = c;
            return View("Option1");          
        }

        public IActionResult GetInvidual()
        {
            string s = Request.Query["id"];
            var c = '3';
            ViewBag.co = c;
            var a = _manager.GetClass("Thing");
            var allIn = _manager.GetClassIndividuals(s, GetAll);        
            ViewBag.allInvidual = allIn; 
            return View("Option1");
        }

        public IActionResult Things()
        {
            
            string s = Request.Query["id"];
           
                var c = '2';
                ViewBag.co = c;
                var a = _manager.GetClass("Thing");               
                var sub = a.DirectSubClasses;
                ViewBag.classsub = sub;                                        
           return View("Option1");
        }

        public IActionResult GetDirectSubClass()
        {
            var c = '4';
            string s = Request.Query["id"];
            var directSubClasses = _manager.GetSubClasses(s, GetDirect);
            if (directSubClasses != null)
            {
                ViewBag.directSubClasses = directSubClasses;
                ViewBag.co = c;
            }
            else
            {
                c = '5';
                String getIn = "/Ontology/Things?id=" + s;
                ViewBag.getIn = s;
                ViewBag.co = c;
            }

            return View("Option1");
        }

        public IActionResult ShowRelation()
        {
           string s = Request.Query["id"];
            var c = '1';
            ViewBag.co = c;
            var a = _manager.GetClass("Thing");
            var relationName = _manager.GetRelation(s);
           
            var inverseRelation = _manager.GetInverseRelation(s);

            var directDomains = _manager.GetRelationDomains(s, GetDirect);
            var allDomains = _manager.GetRelationDomains(s, GetAll);

            var directRanges = _manager.GetRelationRanges(s, GetDirect);
            var allRanges = _manager.GetRelationRanges(s, GetAll);

            ViewBag.inverseRelation = inverseRelation;
            ViewBag.directDomains = directDomains;
            ViewBag.allDomains = allDomains;
            ViewBag.directRanges = directRanges;
            ViewBag.allRanges = allRanges;
            
            return View("partialView");
        }
        public IActionResult ShowAttribultResult()
        {
            string s = Request.Query["id"];
            var c = '3';
            ViewBag.co = c;
            var a = _manager.GetClass("Thing");           
            var attribute = _manager.GetAttribute(s);
            var directDomains = _manager.GetAttributeDomains(s, GetDirect);
            var allDomains = _manager.GetAttributeDomains(s, GetAll);
            ViewBag.attribute = attribute;
            ViewBag.directDomains = directDomains;
            ViewBag.allDomains = allDomains;

            return View("partialView");
        }

        public IActionResult ShowInvidual()
        {
           
            //string s = id;
            string s = Request.Query["id"];
            var c = '2';
            ViewBag.co = c;
            var directClass = _manager.GetIndividualClass(s);
            var allClasses = _manager.GetIndividualClasses(s);

            var relationValues = _manager.GetRelationValues(s);
            var attributeValues = _manager.GetAttributeValues(s);
                   
            var tmpSttributeValues = new Dictionary<Attribute, List<string>>();
            
                foreach (KeyValuePair<Attribute, IReadOnlyCollection<string>> keyIterm in attributeValues)
            {
                Attribute attribute = keyIterm.Key;
                var actualHtlms = new List<string>();
                foreach (var semanticCode in keyIterm.Value)
                {
                    
                    var lexer = new SemanticLexer(semanticCode);
                    var parser = new SemanticParser(lexer);

                    var actualHtml = parser.Parse().ToString();
                    actualHtlms.Add(actualHtml);
                }

                tmpSttributeValues.Add(attribute, actualHtlms);
            }

           // string value = attributeValues.Values.ToString();
            ViewBag.directClass = directClass;
            ViewBag.allClasses = directClass;

            ViewBag.Value = tmpSttributeValues;
            return View("partialView");       
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
