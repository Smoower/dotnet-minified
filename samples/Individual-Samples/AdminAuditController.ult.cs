using Rasepi.Api.Authorization;
using Rasepi.Api.Services.Tenants;
using Rasepi.Shared.Models;
using Rasepi.Shared.Models.Plugins.Classification;

namespace Rasepi.Api.Controllers;

// ULTRA variant. NOT [Crud<>] - this controller is read-only paged audit queries,
// not entity CRUD, so the convention that fits is "paged-filtered-list". It needs
// no source generator: two proposed valid-C# terminators carry the whole pattern.
//   whereIf(cond,pred)                 -> conditional .Where (skips the if/reassign)
//   paged(page,pageSize,sel,ct,max)    -> clamp + CountAsync + Skip/Take + Select
//                                         + Ok(new{items,total,page,pageSize})
//   paged(page,pageSize,ct,max)        -> same, items are the entities (no projection)
// What stays is the contract: routes, filter predicates, and the projections. Those
// are irreducible - they ARE the API. See AdminAuditController.cs / .min.cs to diff.
[AUTH,RequireTenantAdmin,API,RT("admin/audit")]
public class AdminAuditController(RasepiDbContext db,TenantContext tenant):Ctl{
 [HG("expiry-config")]public Tr GetExpiryConfigAudit([FQ]Guid? entryId,[FQ]Guid? userId,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.ExpiryConfigAuditLogs.nt().inc(l=>l.Entry).inc(l=>l.ChangedBy).inc(l=>l.OldTemplate).inc(l=>l.NewTemplate)
   .whereIf(entryId.HasValue,l=>l.EntryId==entryId).whereIf(userId.HasValue,l=>l.ChangedByUserId==userId)
   .whereIf(since.HasValue,l=>l.ChangedAt>=since.Value).whereIf(until.HasValue,l=>l.ChangedAt<=until.Value).obd(l=>l.ChangedAt)
   .paged(page,pageSize,l=>new{l.Id,l.EntryId,entryKey=l.Entry.Key,changeType=l.ChangeType.ToString(),l.OldTemplateId,oldTemplateName=l.OldTemplate!=null?l.OldTemplate.Name:null,l.NewTemplateId,newTemplateName=l.NewTemplate!=null?l.NewTemplate.Name:null,l.OldCustomConfig,l.NewCustomConfig,l.Reason,l.ChangedByUserId,changedByName=l.ChangedBy.Name,changedByEmail=l.ChangedBy.Email,l.ChangedAt},ct);

 [HG("review-history")]public Tr GetReviewHistory([FQ]Guid? entryId,[FQ]Guid? hubId,[FQ]Guid? reviewerId,[FQ]ReviewType? reviewType,[FQ]bool? attestationSigned,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.DocumentReviewHistories.nt().inc(h=>h.Entry).inc(h=>h.Reviewer)
   .whereIf(entryId.HasValue,h=>h.EntryId==entryId).whereIf(hubId.HasValue,h=>h.Entry.HubId==hubId).whereIf(reviewerId.HasValue,h=>h.ReviewerId==reviewerId)
   .whereIf(reviewType.HasValue,h=>h.ReviewType==reviewType).whereIf(attestationSigned.HasValue,h=>h.AttestationSigned==attestationSigned)
   .whereIf(since.HasValue,h=>h.ReviewedAt>=since.Value).whereIf(until.HasValue,h=>h.ReviewedAt<=until.Value).obd(h=>h.ReviewedAt)
   .paged(page,pageSize,h=>new{h.Id,h.EntryId,entryKey=h.Entry.Key,hubId=h.Entry.HubId,h.ReviewerId,reviewerName=h.Reviewer.Name,reviewerEmail=h.Reviewer.Email,h.ReviewedAt,reviewType=h.ReviewType.ToString(),h.ReviewNotes,h.ContentWasUpdated,h.BlocksChanged,h.ExpiryExtended,h.NewExpiryDate,h.TimeSpentSeconds,h.AttestationSigned},ct);

 [HG("notifications")]public Tr GetNotifications([FQ]Guid? entryId,[FQ]Guid? recipientId,[FQ]DeliveryStatus? status,[FQ]NotificationMethod? method,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.ExpiryNotifications.nt().inc(n=>n.Entry).inc(n=>n.Recipient)
   .whereIf(entryId.HasValue,n=>n.EntryId==entryId).whereIf(recipientId.HasValue,n=>n.RecipientUserId==recipientId)
   .whereIf(status.HasValue,n=>n.Status==status).whereIf(method.HasValue,n=>n.Method==method)
   .whereIf(since.HasValue,n=>n.SentAt>=since.Value).whereIf(until.HasValue,n=>n.SentAt<=until.Value).obd(n=>n.SentAt)
   .paged(page,pageSize,n=>new{n.Id,n.EntryId,entryKey=n.Entry.Key,n.RecipientUserId,recipientName=n.Recipient.Name,recipientEmail=n.Recipient.Email,n.DaysBeforeExpiry,n.SentAt,method=n.Method.ToString(),n.NotificationChannelId,status=n.Status.ToString(),n.ErrorMessage,n.Acknowledged,n.AcknowledgedAt},ct);

 [HG("entry-versions")]public Tr GetEntryVersions([FQ]Guid? entryId,[FQ]Guid? hubId,[FQ]Guid? savedById,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.EntryVersions.nt().inc(v=>v.Entry).inc(v=>v.SavedBy)
   .whereIf(entryId.HasValue,v=>v.EntryId==entryId).whereIf(hubId.HasValue,v=>v.Entry.HubId==hubId).whereIf(savedById.HasValue,v=>v.SavedById==savedById)
   .whereIf(since.HasValue,v=>v.SavedAt>=since.Value).whereIf(until.HasValue,v=>v.SavedAt<=until.Value).obd(v=>v.SavedAt)
   .paged(page,pageSize,v=>new{v.Id,v.EntryId,entryKey=v.Entry.Key,hubId=v.Entry.HubId,v.VersionNumber,v.Title,v.SavedById,savedByName=v.SavedBy!=null?v.SavedBy.Name:null,v.SavedAt},ct);

 [HG("translation-history")]public Tr GetTranslationHistory([FQ]Guid? entryId,[FQ]string? provider,[FQ]bool? success,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.TranslationHistories.nt()
   .whereIf(entryId.HasValue,h=>h.EntryId==entryId).whereIf(!provider.nil(),h=>h.Provider==provider).whereIf(success.HasValue,h=>h.Success==success)
   .whereIf(since.HasValue,h=>h.OccurredAt>=since.Value).whereIf(until.HasValue,h=>h.OccurredAt<=until.Value).obd(h=>h.OccurredAt).paged(page,pageSize,ct);

 [HG("classification")]public Tr GetClassificationAudit([FQ]Guid? entryId,[FQ]Guid? performedByUserId,[FQ]string? actionType,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default)=>
  db.Set<EntryClassificationAuditLog>().nt()
   .whereIf(entryId.HasValue,c=>c.EntryId==entryId).whereIf(performedByUserId.HasValue,c=>c.PerformedByUserId==performedByUserId).whereIf(!actionType.nil(),c=>c.ActionType==actionType)
   .whereIf(since.HasValue,c=>c.PerformedAtUtc>=since.Value).whereIf(until.HasValue,c=>c.PerformedAtUtc<=until.Value).obd(c=>c.PerformedAtUtc).paged(page,pageSize,ct);

 // BOUNDARY: 'activity' and 'ai-prompts' join a platform-level User in memory (User
 // lives outside this DbContext), so the actor name can't be an EF projection. The
 // paged-list convention stops here; the body stays hand-written (Tier-2). whereIf
 // still removes the filter ceremony; paging + the in-memory join do not collapse.
 [HG("activity")]public async Tr GetActivity([FQ]string? eventNamePrefix,[FQ]Guid? entityId,[FQ]Guid? actorUserId,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var q=db.AuditLogEntries.nt()
   .whereIf(!eventNamePrefix.nil(),a=>a.EventName.StartsWith(eventNamePrefix!))
   .whereIf(entityId.HasValue,a=>a.EntityId==entityId).whereIf(actorUserId.HasValue,a=>a.ActorUserId==actorUserId)
   .whereIf(since.HasValue,a=>a.OccurredAt>=since.Value).whereIf(until.HasValue,a=>a.OccurredAt<=until.Value);
  var total=await q.cnt(ct);
  var rawItems=await q.obd(a=>a.OccurredAt).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  var userIds=rawItems.Where(a=>a.ActorUserId.HasValue).Select(a=>a.ActorUserId!.Value).Distinct().ToList();
  var userLookup=await db.Users.nt().w(u=>userIds.Contains(u.Id)).s(u=>new{u.Id,u.Name,u.Email}).ToDictionaryAsync(u=>u.Id,ct);
  var items=rawItems.Select(a=>new{a.Id,a.EventName,a.EntityId,a.ActorUserId,actorName=a.ActorUserId.HasValue&&userLookup.TryGetValue(a.ActorUserId.Value,out var u)?u.Name:null,actorEmail=a.ActorUserId.HasValue&&userLookup.TryGetValue(a.ActorUserId.Value,out var u2)?u2.Email:null,a.PayloadType,a.PayloadJson,a.EventVersion,a.CorrelationId,a.OccurredAt});
  return Ok(new{items,total,page,pageSize});
 }

 [HG("ai-prompts")]public async Tr GetAIPromptAudit([FQ]Guid? userId,[FQ]QuotaType? queryType,[FQ]DateTime? since,[FQ]DateTime? until,[FQ]int page=1,[FQ]int pageSize=50,CT ct=default){
  pageSize=Math.Clamp(pageSize,1,200);page=Math.Max(page,1);
  var q=db.AIPromptLogs.nt()
   .whereIf(userId.HasValue,l=>l.UserId==userId).whereIf(queryType.HasValue,l=>l.QueryType==queryType)
   .whereIf(since.HasValue,l=>l.CreatedAt>=since.Value).whereIf(until.HasValue,l=>l.CreatedAt<=until.Value);
  var total=await q.cnt(ct);
  var rawItems=await q.obd(l=>l.CreatedAt).sk((page-1)*pageSize).tk(pageSize).lst(ct);
  var userIds=rawItems.Select(i=>i.UserId).Distinct().ToList();
  var userLookup=await db.Users.nt().w(u=>userIds.Contains(u.Id)).s(u=>new{u.Id,u.Name,u.Email}).ToDictionaryAsync(u=>u.Id,ct);
  var items=rawItems.Select(l=>new{l.Id,l.UserId,userName=userLookup.TryGetValue(l.UserId,out var u)?u.Name:null,userEmail=userLookup.TryGetValue(l.UserId,out var u2)?u2.Email:null,queryType=l.QueryType.ToString(),l.PromptHash,model=l.ModelUsed,l.InputTokens,l.OutputTokens,l.ResponseTimeMs,l.CreatedAt});
  return Ok(new{items,total,page,pageSize});
 }
}
