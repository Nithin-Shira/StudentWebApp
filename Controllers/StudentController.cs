using Microsoft.AspNetCore.Mvc;
using System.Text;

public class StudentController : Controller
{
    private static List<Student> students = new List<Student>()
    {
        new Student { Id = 1, FullName="Raghu", Email="raghu@gmail.com", Course="English", Phone="9999999999", City="Bengaluru", FeePaid=23434, AdmissionDate = DateTime.Today.AddMonths(-2) },
        new Student { Id = 2, FullName="Ram", Email="Ram@gmail.com", Course="English", Phone="8888888888", City="Bengaluru", FeePaid=23434, AdmissionDate = DateTime.Today.AddMonths(-1) }
    };

    const int PageSize = 10;

    // Index: shows cards + chart + pagination + search
    public IActionResult Index(string search, int page = 1)
    {
        var query = students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s) ||
                x.Course.ToLower().Contains(s) ||
                x.City.ToLower().Contains(s));
        }

        int total = query.Count();
        var pageList = query
            .OrderByDescending(x => x.AdmissionDate)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        ViewBag.Search = search ?? "";

        // is admin?
        ViewBag.IsAdmin = AccountController.IsAdmin(HttpContext);

        return View(pageList);
    }

    [HttpGet]
    public IActionResult Add()
    {
        if (!AccountController.IsAdmin(HttpContext))
            return RedirectToAction("Login", "Account", new { returnUrl = "/Student/Add" });

        return View(new Student { AdmissionDate = DateTime.Today });
    }

    [HttpPost]
    public IActionResult Add(Student s)
    {
        if (!AccountController.IsAdmin(HttpContext))
            return RedirectToAction("Login", "Account", new { returnUrl = "/Student/Add" });

        s.Id = students.Count > 0 ? students.Max(x => x.Id) + 1 : 1;
        students.Add(s);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (!AccountController.IsAdmin(HttpContext))
            return RedirectToAction("Login", "Account", new { returnUrl = $"/Student/Edit/{id}" });

        var st = students.FirstOrDefault(x => x.Id == id);
        if (st == null) return NotFound();
        return View(st);
    }

    [HttpPost]
    public IActionResult Edit(Student s)
    {
        if (!AccountController.IsAdmin(HttpContext))
            return RedirectToAction("Login", "Account");

        var old = students.FirstOrDefault(x => x.Id == s.Id);
        if (old == null) return NotFound();

        old.FullName = s.FullName;
        old.Email = s.Email;
        old.Course = s.Course;
        old.Phone = s.Phone;
        old.City = s.City;
        old.FeePaid = s.FeePaid;
        old.AdmissionDate = s.AdmissionDate;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var st = students.FirstOrDefault(x => x.Id == id);
        if (st == null) return NotFound();
        ViewBag.IsAdmin = AccountController.IsAdmin(HttpContext);
        return View(st);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!AccountController.IsAdmin(HttpContext))
            return RedirectToAction("Login", "Account");

        students.RemoveAll(x => x.Id == id);
        return RedirectToAction(nameof(Index));
    }

    // CSV export (no packages) - uses current search filter
    public IActionResult ExportCsv(string search)
    {
        var query = students.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s));
        }

        var list = query.OrderByDescending(x => x.AdmissionDate).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Id,FullName,Email,Course,Phone,City,AdmissionDate,FeePaid");
        foreach (var st in list)
        {
            var line = $"{st.Id},\"{st.FullName}\",\"{st.Email}\",\"{st.Course}\",\"{st.Phone}\",\"{st.City}\",{st.AdmissionDate:yyyy-MM-dd},{st.FeePaid}";
            sb.AppendLine(line);
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "students.csv");
    }

    // Chart data endpoint for Chart.js: returns fees per course and counts
    public IActionResult ChartData()
    {
        var byCourse = students
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Course) ? "Unknown" : s.Course)
            .Select(g => new { Course = g.Key, TotalFee = g.Sum(x => x.FeePaid), Count = g.Count() })
            .ToList();

        return Json(byCourse);
    }
}
