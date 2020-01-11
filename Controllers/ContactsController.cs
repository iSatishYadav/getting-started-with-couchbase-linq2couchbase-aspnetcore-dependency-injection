using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Linq;
using HelloCouch.Data;
using HelloCouch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelloCouch.Controllers
{
    public class ContactsController : Controller
    {
        private BucketContext _bucketContext;
        public ContactsController(BucketContext bucketContext)
        {
            _bucketContext = bucketContext;
        }

        // GET: Contacts
        public ActionResult Index()
        {            
            var allContacts = _bucketContext
                .Query<Contact>()
                .Where(x => x.Type == "Contact")
                .Select(x => new ContactDto
                {
                    Id = N1QlFunctions.Meta(x).Id,
                    Name = x.Name,
                    Number = x.Number
                })
                .ToList();

            return View(allContacts);
        }

        // GET: Contacts/Details/5
        public ActionResult Details(string id)
        {
            var contact = _bucketContext
                .Query<Contact>()
                .Select(x => new ContactDto
                {
                    Id = N1QlFunctions.Meta(x).Id,
                    Name = x.Name,
                    Number = x.Number
                })
                .FirstOrDefault(x => x.Id == id);
            return View(contact);
        }

        // GET: Contacts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContactDto contactDto)
        {
            try
            {
                // TODO: Add insert logic here
                //contactsModel.Id = Guid.NewGuid().ToString();
                var contact = new Contact { Name = contactDto.Name, Number = contactDto.Number };
                _bucketContext.Save(contact);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.TryAddModelException("exception", e);
                return View();
            }
        }

        // GET: Contacts/Edit/5
        public ActionResult Edit(string id)
        {
            var contact = _bucketContext
                .Query<Contact>()
                .Select(x => new ContactDto
                {
                    Id = N1QlFunctions.Meta(x).Id,
                    Name = x.Name,
                    Number = x.Number
                })
                .FirstOrDefault(x => x.Id == id);
            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ContactDto contact)
        {
            try
            {
                // TODO: Add update logic here                
                _bucketContext.Save(new Contact { Name = contact.Name, Number = contact.Number });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                ModelState.TryAddModelException("exception", e);
                return View();
            }
        }

        // GET: Contacts/Delete/5\        
        [HttpGet(Name = "Delete")]
        public ActionResult ConfirmDelete(string id)
        {
            var contact = _bucketContext
                .Query<Contact>()
                .Select(x => new ContactDto
                {
                    Id = N1QlFunctions.Meta(x).Id,
                    Name = x.Name,
                    Number = x.Number
                })
                .FirstOrDefault(x => x.Id == id);
            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            try
            {
                // TODO: Add delete logic here
                var contact = _bucketContext.Query<Contact>().FirstOrDefault(x => N1QlFunctions.Meta(x).Id == id);
                if (contact == null)
                    return NotFound();
                _bucketContext.Remove(contact);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}