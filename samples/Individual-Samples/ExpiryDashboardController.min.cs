using Microsoft.EntityFrameworkCore;
using Rasepi.Api.Authorization;
using Rasepi.Api.Data;
using Rasepi.Api.Services.Freshness;
using Rasepi.Shared.Models;

namespace Rasepi.Api.Controllers;

[AUTH,API,RT("expiry-dashboard")]
public class ExpiryDashboardController(RasepiDbContext db,IAuthorizationService authService,IExpiryService expiryService):Ctl{
 [HG("me")][P200<PersonalExpiryDashboard>][P401]public async Tr GetPersonalDashboard(){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  var entries=await db.Entries.w(e=>e.OwnerId==userId.Value&&e.DeletedAt==null).inc(e=>e.Hub).inc(e=>e.Translations.Where(t=>t.DeletedAt==null)).lst();
  var now=Clk.utc;
  var details=new List<HubEntryExpiryDetail>();
  foreach(var entry in entries){
   var title=entry.Translations.FirstOrDefault(t=>t.Language==entry.OriginalLanguage)?.Title??entry.Translations.FirstOrDefault()?.Title;
   DateTime? expiryDate=await expiryService.CalculateExpiryDateAsync(entry.Id);
   int? daysUntil=expiryDate.HasValue?(int)(expiryDate.Value-now).TotalDays:(int?)null;
   details.Add(new HubEntryExpiryDetail(entry.Id,entry.Key,title,entry.Status.ToString(),expiryDate,daysUntil,entry.Hub.Key,entry.Hub.Name));
  }
  details=details.OrderBy(e=>e.DaysUntilExpiry??int.MaxValue).ToList();
  var summary=new PersonalExpirySummary(details.Count,details.Count(e=>(e.DaysUntilExpiry??1)<=0),details.Count(e=>e.DaysUntilExpiry is>0 and<=7),details.Count(e=>e.DaysUntilExpiry is>7 and<=30),details.Count(e=>e.DaysUntilExpiry==null||e.DaysUntilExpiry>30));
  return Ok(new PersonalExpiryDashboard(summary,details));
 }
 [HG("summary")][RequireGlobalAdmin][P200<IEnumerable<HubExpirySummary>>]public async Tr GetTenantSummary(){
  var hubs=await db.Hubs.w(h=>h.DeletedAt==null).s(h=>new{h.Id,h.Key,h.Name}).lst();
  if(!hubs.Any())return Ok(Array.Empty<HubExpirySummary>());
  var hubIds=hubs.Select(h=>h.Id).ToHashSet();
  var entryCounts=await db.Entries.w(e=>e.DeletedAt==null&&hubIds.Contains(e.HubId)).GroupBy(e=>new{e.HubId,e.Status}).s(g=>new{g.Key.HubId,g.Key.Status,Count=g.Count()}).lst();
  var result=hubs.Select(hub=>{
   var counts=entryCounts.Where(c=>c.HubId==hub.Id).ToList();
   return new HubExpirySummary(hub.Id,hub.Key,hub.Name,counts.Sum(c=>c.Count),counts.Where(c=>c.Status==EntryStatus.Expired).Sum(c=>c.Count),counts.Where(c=>c.Status==EntryStatus.ExpiringSoon).Sum(c=>c.Count),counts.Where(c=>c.Status==EntryStatus.Warning).Sum(c=>c.Count),counts.Where(c=>c.Status is EntryStatus.Published or EntryStatus.Draft).Sum(c=>c.Count));
  }).OrderByDescending(h=>h.ExpiredCount+h.ExpiringThisWeekCount);
  return Ok(result);
 }
 [HG("hubs/{hubKey}/entries")][RequireHubRole(PermissionLevel.Admin)][P200<IEnumerable<HubEntryExpiryDetail>>][P404]public async Tr GetHubEntries(string hubKey){
  var hub=await db.Hubs.w(h=>h.Key==hubKey&&h.DeletedAt==null).one();
  if(hub==null)return NotFound();
  var entries=await db.Entries.w(e=>e.HubId==hub.Id&&e.DeletedAt==null).inc(e=>e.Translations.Where(t=>t.DeletedAt==null)).lst();
  var now=Clk.utc;
  var result=new List<HubEntryExpiryDetail>();
  foreach(var entry in entries){
   var title=entry.Translations.FirstOrDefault(t=>t.Language==entry.OriginalLanguage)?.Title??entry.Translations.FirstOrDefault()?.Title;
   DateTime? expiryDate=await expiryService.CalculateExpiryDateAsync(entry.Id);
   int? daysUntil=expiryDate.HasValue?(int)(expiryDate.Value-now).TotalDays:(int?)null;
   result.Add(new HubEntryExpiryDetail(entry.Id,entry.Key,title,entry.Status.ToString(),expiryDate,daysUntil,hub.Key,hub.Name));
  }
  return Ok(result.OrderBy(e=>e.DaysUntilExpiry??int.MaxValue));
 }
}
public record HubExpirySummary(Guid HubId,string HubKey,string HubName,int TotalEntries,int ExpiredCount,int ExpiringThisWeekCount,int ExpiringThisMonthCount,int HealthyCount);
public record HubEntryExpiryDetail(Guid EntryId,string EntryKey,string? Title,string Status,DateTime? ExpiryDate,int? DaysUntilExpiry,string HubKey,string HubName);
public record PersonalExpirySummary(int TotalOwned,int OverdueCount,int ExpiringThisWeekCount,int ExpiringThisMonthCount,int HealthyCount);
public record PersonalExpiryDashboard(PersonalExpirySummary Summary,IEnumerable<HubEntryExpiryDetail> Entries);
