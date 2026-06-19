using Microsoft.EntityFrameworkCore;
using Rasepi.Api.Authorization;
using Rasepi.Api.Data;
using Rasepi.Api.Services.Tenants;
using Rasepi.Shared.Models;
using Rasepi.Shared.Models.Plugins.Classification;

namespace Rasepi.Api.Controllers;

[AUTH,RequireTenantAdmin,API,RT("admin/audit")]
public class AdminAuditController(RasepiDbContext db,TenantContext tenantContext):Ctl{
 [HG("expiry-config")]public async Tr GetExpiryConfigAudit([FQ]Guid? entryId,[FQ]Guid? userId,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.ExpiryConfigAuditLogs.nt().inc(l=>l.Entry).inc(l=>l.ChangedBy).inc(l=>l.OldTemplate).inc(l=>l.NewTemplate).AsQueryable();
  if(entryId.HasValue)query=query.w(l=>l.EntryId==entryId.Value);
  if(userId.HasValue)query=query.w(l=>l.ChangedByUserId==userId.Value);
  if(since.HasValue)query=query.w(l=>l.ChangedAt>=since.Value);
  if(until.HasValue)query=query.w(l=>l.ChangedAt<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(l=>l.ChangedAt).sk((page-1)*pageSize).tk(pageSize).s(l=>new{l.Id,l.EntryId,entryKey=l.Entry.Key,changeType=l.ChangeType.ToString(),l.OldTemplateId,oldTemplateName=l.OldTemplate!=null?l.OldTemplate.Name:null,l.NewTemplateId,newTemplateName=l.NewTemplate!=null?l.NewTemplate.Name:null,l.OldCustomConfig,l.NewCustomConfig,l.Reason,l.ChangedByUserId,changedByName=l.ChangedBy.Name,changedByEmail=l.ChangedBy.Email,l.ChangedAt}).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("review-history")]public async Tr GetReviewHistory([FQ]Guid? entryId,[FQ]Guid? hubId,[FQ]Guid? reviewerId,[FQ]ReviewType? reviewType,[FQ]bool? attestationSigned,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.DocumentReviewHistories.nt().inc(h=>h.Entry).inc(h=>h.Reviewer).AsQueryable();
  if(entryId.HasValue)query=query.w(h=>h.EntryId==entryId.Value);
  if(hubId.HasValue)query=query.w(h=>h.Entry.HubId==hubId.Value);
  if(reviewerId.HasValue)query=query.w(h=>h.ReviewerId==reviewerId.Value);
  if(reviewType.HasValue)query=query.w(h=>h.ReviewType==reviewType.Value);
  if(attestationSigned.HasValue)query=query.w(h=>h.AttestationSigned==attestationSigned.Value);
  if(since.HasValue)query=query.w(h=>h.ReviewedAt>=since.Value);
  if(until.HasValue)query=query.w(h=>h.ReviewedAt<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(h=>h.ReviewedAt).sk((page-1)*pageSize).tk(pageSize).s(h=>new{h.Id,h.EntryId,entryKey=h.Entry.Key,hubId=h.Entry.HubId,h.ReviewerId,reviewerName=h.Reviewer.Name,reviewerEmail=h.Reviewer.Email,h.ReviewedAt,reviewType=h.ReviewType.ToString(),h.ReviewNotes,h.ContentWasUpdated,h.BlocksChanged,h.ExpiryExtended,h.NewExpiryDate,h.TimeSpentSeconds,h.AttestationSigned}).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("notifications")]public async Tr GetNotifications([FQ]Guid? entryId,[FQ]Guid? recipientId,[FQ]DeliveryStatus? status,[FQ]NotificationMethod? method,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.ExpiryNotifications.nt().inc(n=>n.Entry).inc(n=>n.Recipient).AsQueryable();
  if(entryId.HasValue)query=query.w(n=>n.EntryId==entryId.Value);
  if(recipientId.HasValue)query=query.w(n=>n.RecipientUserId==recipientId.Value);
  if(status.HasValue)query=query.w(n=>n.Status==status.Value);
  if(method.HasValue)query=query.w(n=>n.Method==method.Value);
  if(since.HasValue)query=query.w(n=>n.SentAt>=since.Value);
  if(until.HasValue)query=query.w(n=>n.SentAt<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(n=>n.SentAt).sk((page-1)*pageSize).tk(pageSize).s(n=>new{n.Id,n.EntryId,entryKey=n.Entry.Key,n.RecipientUserId,recipientName=n.Recipient.Name,recipientEmail=n.Recipient.Email,n.DaysBeforeExpiry,n.SentAt,method=n.Method.ToString(),n.NotificationChannelId,status=n.Status.ToString(),n.ErrorMessage,n.Acknowledged,n.AcknowledgedAt}).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("activity")]public async Tr GetActivity([FQ]string? eventNamePrefix,[FQ]Guid? entityId,[FQ]Guid? actorUserId,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.AuditLogEntries.nt();
  if(!eventNamePrefix.nil())query=query.w(a=>a.EventName.StartsWith(eventNamePrefix!));
  if(entityId.HasValue)query=query.w(a=>a.EntityId==entityId.Value);
  if(actorUserId.HasValue)query=query.w(a=>a.ActorUserId==actorUserId.Value);
  if(since.HasValue)query=query.w(a=>a.OccurredAt>=since.Value);
  if(until.HasValue)query=query.w(a=>a.OccurredAt<=until.Value);
  var total=await query.cnt(ct);
  var rawItems=await query.obd(a=>a.OccurredAt).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  var userIds=rawItems.Where(a=>a.ActorUserId.HasValue).Select(a=>a.ActorUserId!.Value).Distinct().ToList();
  var userLookup=await db.Users.nt().w(u=>userIds.Contains(u.Id)).s(u=>new{u.Id,u.Name,u.Email}).ToDictionaryAsync(u=>u.Id,ct);
  var items=rawItems.Select(a=>new{a.Id,a.EventName,a.EntityId,a.ActorUserId,actorName=a.ActorUserId.HasValue&&userLookup.TryGetValue(a.ActorUserId.Value,out var u)?u.Name:null,actorEmail=a.ActorUserId.HasValue&&userLookup.TryGetValue(a.ActorUserId.Value,out var u2)?u2.Email:null,a.PayloadType,a.PayloadJson,a.EventVersion,a.CorrelationId,a.OccurredAt});
  return Ok(new{items,total,page,pageSize});
 }
 [HG("entry-versions")]public async Tr GetEntryVersions([FQ]Guid? entryId,[FQ]Guid? hubId,[FQ]Guid? savedById,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.EntryVersions.nt().inc(v=>v.Entry).inc(v=>v.SavedBy).AsQueryable();
  if(entryId.HasValue)query=query.w(v=>v.EntryId==entryId.Value);
  if(hubId.HasValue)query=query.w(v=>v.Entry.HubId==hubId.Value);
  if(savedById.HasValue)query=query.w(v=>v.SavedById==savedById.Value);
  if(since.HasValue)query=query.w(v=>v.SavedAt>=since.Value);
  if(until.HasValue)query=query.w(v=>v.SavedAt<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(v=>v.SavedAt).sk((page-1)*pageSize).tk(pageSize).s(v=>new{v.Id,v.EntryId,entryKey=v.Entry.Key,hubId=v.Entry.HubId,v.VersionNumber,v.Title,v.SavedById,savedByName=v.SavedBy!=null?v.SavedBy.Name:null,v.SavedAt}).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("translation-history")]public async Tr GetTranslationHistory([FQ]Guid? entryId,[FQ]string? provider,[FQ]bool? success,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.TranslationHistories.nt();
  if(entryId.HasValue)query=query.w(h=>h.EntryId==entryId.Value);
  if(!provider.nil())query=query.w(h=>h.Provider==provider);
  if(success.HasValue)query=query.w(h=>h.Success==success.Value);
  if(since.HasValue)query=query.w(h=>h.OccurredAt>=since.Value);
  if(until.HasValue)query=query.w(h=>h.OccurredAt<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(h=>h.OccurredAt).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("classification")]public async Tr GetClassificationAudit([FQ]Guid? entryId,[FQ]Guid? performedByUserId,[FQ]string? actionType,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.Set<EntryClassificationAuditLog>().nt();
  if(entryId.HasValue)query=query.w(c=>c.EntryId==entryId.Value);
  if(performedByUserId.HasValue)query=query.w(c=>c.PerformedByUserId==performedByUserId.Value);
  if(!actionType.nil())query=query.w(c=>c.ActionType==actionType);
  if(since.HasValue)query=query.w(c=>c.PerformedAtUtc>=since.Value);
  if(until.HasValue)query=query.w(c=>c.PerformedAtUtc<=until.Value);
  var total=await query.cnt(ct);
  var items=await query.obd(c=>c.PerformedAtUtc).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  return Ok(new{items,total,page,pageSize});
 }
 [HG("ai-prompts")]public async Tr GetAIPromptAudit([FQ]Guid? userId,[FQ]QuotaType? queryType,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var query=db.AIPromptLogs.nt();
  if(userId.HasValue)query=query.w(l=>l.UserId==userId.Value);
  if(queryType.HasValue)query=query.w(l=>l.QueryType==queryType.Value);
  if(since.HasValue)query=query.w(l=>l.CreatedAt>=since.Value);
  if(until.HasValue)query=query.w(l=>l.CreatedAt<=until.Value);
  var total=await query.cnt(ct);
  var rawItems=await query.obd(l=>l.CreatedAt).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  var userIds=rawItems.Select(i=>i.UserId).Distinct().ToList();
  var userLookup=await db.Users.nt().w(u=>userIds.Contains(u.Id)).s(u=>new{u.Id,u.Name,u.Email}).ToDictionaryAsync(u=>u.Id,ct);
  var items=rawItems.Select(l=>new{l.Id,l.UserId,userName=userLookup.TryGetValue(l.UserId,out var u)?u.Name:null,userEmail=userLookup.TryGetValue(l.UserId,out var u2)?u2.Email:null,queryType=l.QueryType.ToString(),l.PromptHash,model=l.ModelUsed,l.InputTokens,l.OutputTokens,l.ResponseTimeMs,l.CreatedAt});
  return Ok(new{items,total,page,pageSize});
 }
}
