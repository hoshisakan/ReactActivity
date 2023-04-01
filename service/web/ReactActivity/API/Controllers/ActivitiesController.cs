using Domain;
using Persistence;
using Persistence.Repository.IRepository;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    public class ActivitiesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivitiesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<List<Activity>>> GetActivities()
        {
            // return await _unitOfWork.Activity.GetAll(orderBy: a => a.OrderByDescending(x => x.CreatedAt));
            return await _unitOfWork.Activity.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(Guid id)
        {
            Activity activity = await _unitOfWork.Activity.GetById(id);
            if (activity == null)
            {
                return NotFound();
            }
            return activity;
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity(Activity activity)
        {
            activity.CreatedAt = DateTime.Now;

            await _unitOfWork.Activity.Add(activity);
            await _unitOfWork.Save();

            return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActivity(Guid id, Activity activity)
        {
            Activity activityToUpdate = await _unitOfWork.Activity.GetById(id);

            if (activityToUpdate == null)
            {
                return NotFound();
            }

            activityToUpdate.Title = activity.Title;
            activityToUpdate.Description = activity.Description;
            activityToUpdate.Category = activity.Category;
            activityToUpdate.Date = activity.Date;
            activityToUpdate.City = activity.City;
            activityToUpdate.Venue = activity.Venue;
            activityToUpdate.ModifiedAt = DateTime.Now;

            await _unitOfWork.Save();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            Activity activityToDelete = await _unitOfWork.Activity.GetById(id);

            if (activityToDelete == null)
            {
                return NotFound();
            }

            _unitOfWork.Activity.Remove(activityToDelete);
            await _unitOfWork.Save();

            return NoContent();
        }
    }
}