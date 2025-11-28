using System;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Course { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public double FeePaid { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.Today;
}
