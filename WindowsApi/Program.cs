using WindowsApi;

string myUniqueId = HardwareIdGenerator.GetUniqueHardwareId();

Console.WriteLine("Hardware ID:");
Console.WriteLine(myUniqueId);

// example: 
// 4F92E18C9D8F2A3B4C5D6E7F8A9B0C1D

Console.ReadLine();
