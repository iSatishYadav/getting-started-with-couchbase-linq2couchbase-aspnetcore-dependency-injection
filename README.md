# How to get started with [Couchbase][Couchbase Home Page] with ASP.NET Core using [Linq2Couchbase][Linq2Couchbase Github Page] AND Dependency Injection
## Introduction 
This totorial will create a sample repo for using [Couchbase][Couchbase Home Page] with ASP.NET Core 3 with [Linq2Couchbase][Linq2Couchbase Github Page] AND Dependency Injection.

## How to re-create this repo?
### Bucket setup
Assuming you already have an up and running Couchbase server running. If not, chcek-out official Couchbase docs and come back.

Create a new `bucket` named _contacts_ add a few documents like this

````JSON
{
  "name": "Satish K. Yadav",
  "number": "9876543210",
  "type": "Contact"
}
````
> Since this `bucket` may contain any type of document, an optional property `type` has been added to filter `contact` objects easily.
> 
### New Peoject
Create a new Project with ASP.NET Core - Web Application (MVC).
### Installing Couchbase packages

Install following nuget packages.
1.   [`CouchbaseNetClient`][Couchbase SDK Nuget] - Couchbase .NET SDK
2.   [`Couchbase.Extensions.DependencyInjection`][Couchbase Dependency Injection Nuget] - Dependency Injection extensions
3.   [`Linq2Couchbase`][Linq2Couchbase Nuget] - Linq-to-Couchbase provider for accessing database like other ORMs e.g. `EntityFramework`.
### Couchbase Server Configuration
#### Keeping Couchbase configuration in ASP.NET Configuration
Add Couchbase Database Server configuration in `appsettings.json` e.g.
````JSON
{
  "Couchbase": {
    "Servers": [
      "http://localhost"
    ],
    "Username": "USERNAME",
    "Password":  "PASSWORD",
    "UseSsl": false
  }
}
````
> For Production, you may want to set `UseSsl` as `true` depending on your server configuration.

#### Couchbase configuration with ASP.NET Core Dependency Injection
Add configuration to `ConfigureServices` method
````Csharp
services.AddCouchbase(Configuration.GetSection("Couchbase"))
````

### Accessing buckets
Instead of hardcoding bucket names all over the project, use buckets with named providers.
#### Create a named bucket provider
Create new folder `Data` in the root, and add an `interface` named `IContactsBucketProvider`. It should implement `INamedBucketProvider`. Leave it blank. It should look like this.
````CSharp
public interface IContactsBucketProvider: INamedBucketProvider
{
}
````
#### Register bucket provider with Dependency Injection
Register named bucket provider with `AddCouchbaseBucket` by chaining it to `AddCouchbase` call, so it looks like this:
````CSharp
services
    .AddCouchbase(Configuration.GetSection("Couchbase"))
    .AddCouchbaseBucket<IContactsBucketProvider>("contacts");
````
### Couchbase clean-up after application stops
Add a `IHostApplicationLifetime` parameter to `Configure` method and code to clean-up Couchbase once the application stops, so it looks like this:
```CSharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
{
//Other configurations
 hostApplicationLifetime.ApplicationStopped.Register(() =>{
                //Cleaning up using using Dependency Injection
                app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();                
            });
}
````
### Context using `Linq2Couchbase`
Register a `BucketContext` to be used for accessging documents from datbase. In `ConfigureServices` method add following:

````CSharp
 services.AddTransient(x =>
    {
        var contactsBucket = x.GetRequiredService<IContactsBucketProvider>();
        return new BucketContext(contactsBucket.GetBucket());
    });
````

### POCO classes for accessing Couchbase
Add a `Contact` class in `Data` folder for accessing Couchbase documents.
````CSharp
public class Contact
{        
    public string Name { get; set; }
    public string Number { get; set; }
    public string Type => typeof(Contact).Name;
}
````

Now create a Model class under `Models` folder for controllers.
````CSharp
public class ContactDto
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }        
}
````
### Controllers
Add a new `ContactsController` and add all `CRUD` methods, which looks like this:
````CSharp
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

    // GET: Contacts/Delete/5      
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
````
### Views
Add `Index`, `Create`, `Edit`, `Details`, and `Delete` views, under `Views`-> `Contacts` folder. Alternatively generate these views by scaffolding. Views should look something like these:
#### Create

````HTML
@model YOUR_NAMESPACE_HERE.Models.ContactDto

@{
    ViewData["Title"] = "Create";
}

<h1>Create</h1>

<h4>ContactsModel</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>            
            <div class="form-group">
                <label asp-for="Name" class="control-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Number" class="control-label"></label>
                <input asp-for="Number" class="form-control" />
                <span asp-validation-for="Number" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-action="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

````
#### Delete
````HTML
@model YOUR_NAMESPACE_HERE.Models.ContactDto

@{
    ViewData["Title"] = "Delete";
}

<h1>Delete Contact</h1>

<h3>Are you sure you want to delete this?</h3>
<div>    
    <hr />
    <dl class="row">        
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.Name)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Name)
        </dd>
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.Number)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Number)
        </dd>        
    </dl>
    
    <form asp-action="Delete">
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-action="Index">Back to List</a>
    </form>
</div>
````

#### Details
````HTML
@model YOUR_NAMESPACE_HERE.Models.ContactDto

@{
    ViewData["Title"] = "Details";
}

<h1>Contact Details</h1>

<div>    
    <hr />
    <dl class="row">        
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.Name)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Name)
        </dd>
        <dt class = "col-sm-2">
            @Html.DisplayNameFor(model => model.Number)
        </dt>
        <dd class = "col-sm-10">
            @Html.DisplayFor(model => model.Number)
        </dd>        
    </dl>
</div>
<div>
    @Html.ActionLink("Edit", "Edit", new { id = Model.Id }) |
    <a asp-action="Index">Back to List</a>
</div>
````
#### Edit
````HTML
@model YOUR_NAMESPACE_HERE.Models.ContactDto

@{
    ViewData["Title"] = "Edit";
}

<h1>Edit Contact</h1>

<hr />
<div class="row">
    <div class="col-md-4">
        <form asp-action="Edit">
            <div asp-validation-summary="All" class="text-danger"></div>
            <input type="hidden" readonly asp-for="Id" class="form-control" />
            <div class="form-group">
                <label asp-for="Name" class="control-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Number" class="control-label"></label>
                <input asp-for="Number" class="form-control" />
                <span asp-validation-for="Number" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Save" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-action="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

````
#### Index
````HTML
@model IEnumerable<YOUR_NAMESPACE_HERE.Models.ContactDto>

@{
    ViewData["Title"] = "Index";
}

<h1>Index</h1>

<p>
    <a asp-action="Create">Create New</a>
</p>
<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Name)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Number)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>
                    @Html.DisplayFor(modelItem => item.Name)
                </td>
                <td>
                    @Html.DisplayFor(modelItem => item.Number)
                </td>
                <td>
                    @Html.ActionLink("Edit", "Edit", new { id = item.Id }) |
                    @Html.ActionLink("Details", "Details", new { id = item.Id }) |
                    @Html.ActionLink("Delete", "Delete", new { id = item.Id })
                </td>
            </tr>
        }
    </tbody>
</table>
````
#### `jQuery` validation library
`_ValidationScriptsPartial.cshtml` looks like this
````HTML
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
````

### Run
Thant's it! Run and your application is ready to be served.


[Couchbase Home Page]: https://couchbase.com
[Linq2Couchbase Github Page]: https://github.com/couchbaselabs/Linq2Couchbase
[Couchbase SDK Nuget]: https://www.nuget.org/packages/CouchbaseNetClient/
[Linq2Couchbase Nuget]: https://www.nuget.org/packages/Linq2Couchbase/
[Couchbase Dependency Injection Nuget]: https://www.nuget.org/packages/Couchbase.Extensions.DependencyInjection/