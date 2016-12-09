using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }
        // GET: Article List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                // Get Articles from database
                var articles = database
                    .Articles
                    .Include(a => a.Author)
                    .ToList();

                return View(articles);
            }              
        }

        //GET: Article Details
        public  ActionResult Details(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        //POST: Article\Create
        [Authorize]
        public ActionResult Create(Article article)
        {
            if(ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get author Id
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;
                    
                    // Save article's author
                    article.AuthorId = authorId;
                    
                    // Save article in DB
                    database.Articles.Add(article);
                    database.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        //GET: Article\Delete
        public ActionResult Delete(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                // Get article from database
                var article = database.Articles
                                .Where(a => a.Id == id)
                                .Include(a => a.Author)
                                .First();

                // Check authorization
                if(!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                // Check if article exist

                if(article == null)
                {
                    return HttpNotFound();
                }

                // Pass article to view
                return View(article);
            }
        }

        //POST: Article\Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                //Get article from database
                var article = database.Articles
                                .Where(a => a.Id == id)
                                .Include(a => a.Author)
                                .First();

                // Check if article exist
                if(article == null)
                {
                    return HttpNotFound();
                }

                //Delete article from database
                database.Articles.Remove(article);
                database.SaveChanges();

                //Redirect to index page
                return RedirectToAction("Index");
            }
        }

        // GET Article/Edit
        public ActionResult Edit(int? id)
        {
            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                // Get article from database
                var article = database.Articles
                                .Where(a => a.Id == id)
                                .Include(a => a.Author)
                                .First();

                if(!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                // Check if article exist
                if(article == null)
                {
                    return HttpNotFound();
                }
                //Create the view model
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;

                //Pass it to the view
                return View(model);



            }
        }

        //Post Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            //check if modelstate is valid
            if(ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get article from database
                    var article = database.Articles
                                    .FirstOrDefault(a => a.Id == model.Id);
                    
                    //Set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    
                    //Save article state in database
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    //Redirect to the index page
                    return RedirectToAction("Index");
                }
            }

            //if modelstate is invalid return the same view
            return View(model);
        }

        public bool IsUserAuthorizedToEdit (Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.isAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }


}