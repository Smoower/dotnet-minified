using Rasepi.Api.Authorization;

namespace Rasepi.Api.Controllers;

[AUTH,API,RT("entries/{entryId:guid}")]
public class EntryExpiryController(IExpiryService expiryService,IReviewService reviewService,IRenewalService renewalService,IAuthorizationService authService,IExpiryNotificationService notificationService):Ctl{
 [HG("expiry-config")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetExpiryConfig(Guid entryId){
  try{return Ok(await expiryService.GetEffectiveConfigAsync(entryId));}catch(KeyNotFoundException){return NotFound();}
 }
 [HG("effective-template")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetEffectiveTemplate(Guid entryId){
  try{return Ok(await expiryService.GetEffectiveTemplateAsync(entryId));}catch(KeyNotFoundException){return NotFound();}
 }
 [HG("expiry-status")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetExpiryStatus(Guid entryId){
  try{
   var expiryDate=await expiryService.CalculateExpiryDateAsync(entryId);
   var daysUntil=await expiryService.GetDaysUntilExpiryAsync(entryId);
   var inWarning=await expiryService.IsInWarningPeriodAsync(entryId);
   var paused=await expiryService.IsExpiryPausedAsync(entryId);
   return Ok(new{expiryDate,daysUntilExpiry=daysUntil==int.MaxValue?(int?)null:daysUntil,isInWarningPeriod=inWarning,isPaused=paused});
  }catch(KeyNotFoundException){return NotFound();}
 }
 [HPO("change-template")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr ChangeTemplate(Guid entryId,[FB]ChangeTemplateRequest request){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{await expiryService.ChangeTemplateAsync(entryId,request.TemplateId,userId.Value,request.Reason);return NoContent();}catch(KeyNotFoundException){return NotFound();}
 }
 [HPO("set-custom-config")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr SetCustomConfig(Guid entryId,[FB]SetCustomConfigRequest request){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{await expiryService.SetCustomConfigAsync(entryId,request.ConfigJson,userId.Value,request.Reason);return NoContent();}catch(KeyNotFoundException){return NotFound();}
 }
 [HD("custom-config")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr ClearCustomConfig(Guid entryId){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{await expiryService.ClearCustomConfigAsync(entryId,userId.Value);return NoContent();}catch(KeyNotFoundException){return NotFound();}
 }
 [HPO("review")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr CreateReview(Guid entryId,[FB]CreateReviewRequest request){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{
   var review=await reviewService.CreateReviewAsync(entryId,userId.Value,request.ReviewType,request.Notes,request.TimeSpentSeconds,request.BlocksChanged,request.ContentWasUpdated,request.ChecklistCompletionJson);
   return Ok(review);
  }catch(KeyNotFoundException){return NotFound();}catch(InvalidOperationException ex){return UnprocessableEntity(new{error=ex.Message});}
 }
 [HG("review-history")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetReviewHistory(Guid entryId)=>Ok(await reviewService.GetHistoryAsync(entryId));
 [HG("last-review")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetLastReview(Guid entryId){
  var review=await reviewService.GetLastReviewAsync(entryId);return review==null?NotFound():Ok(review);
 }
 [HPO("review/{reviewId:guid}/attest")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr SignAttestation(Guid entryId,Guid reviewId){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{var review=await reviewService.SignAttestationAsync(reviewId,userId.Value);return Ok(review);}catch(KeyNotFoundException){return NotFound();}catch(InvalidOperationException){return Forbid();}
 }
 [HG("review-requirements")][RequireDocumentRole(PermissionLevel.Viewer)]public async Tr GetReviewRequirements(Guid entryId){
  try{return Ok(await reviewService.GetReviewRequirementsAsync(entryId));}catch(KeyNotFoundException){return NotFound();}
 }
 [HPO("renew")][RequireDocumentRole(PermissionLevel.Editor)]public async Tr Renew(Guid entryId,[FB]RenewRequest? request=null){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  try{var doc=await renewalService.RenewAsync(entryId,userId.Value,request?.CustomExpiryDays,request?.Notes);return Ok(doc);}catch(KeyNotFoundException){return NotFound();}catch(InvalidOperationException ex){return UnprocessableEntity(new{error=ex.Message});}
 }
 [HG("renewal-eligibility")]public async Tr GetRenewalEligibility(Guid entryId){
  var userId=authService.GetCurrentUserId(HttpContext);if(userId==null)return Unauthorized();
  var blockers=await renewalService.GetRenewalBlockersAsync(entryId);return Ok(new{canRenew=!blockers.Any(),blockers});
 }
}
public record ChangeTemplateRequest(Guid TemplateId,string? Reason);
public record SetCustomConfigRequest(string ConfigJson,string? Reason);
public record CreateReviewRequest(ReviewType ReviewType,string? Notes,int TimeSpentSeconds,int BlocksChanged=0,bool ContentWasUpdated=false,string? ChecklistCompletionJson=null);
public record RenewRequest(int? CustomExpiryDays,string? Notes);
