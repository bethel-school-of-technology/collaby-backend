using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using collaby_backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using collaby_backend.Helper;

namespace collaby_backend.Controllers
{

    [Route("api/posts")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId(){

            string token = Request.Headers["Authorization"];
            int userId = Int16.Parse(Jwt.decryptJSONWebToken(token)["Id"].ToString());

            return userId;
        }

        [HttpGet] //get all posts
        [AllowAnonymous]
        public ActionResult<IEnumerable<Post>> Post()
        {
            List<Post> PostList = _context.Posts.Where(o=>o.IsDraft != 1).ToList();
            return PostList;
        }

        [HttpGet("user/{username}")] //get posts from sepecific user
        [AllowAnonymous]
        public ActionResult<IEnumerable<Post>> GetUserPosts(string username)
        {
            long userId = _context.Users.First(o=>o.UserName == username).Id;
            List<Post> PostList = _context.Posts.Where(o=>o.UserId == userId && o.IsDraft != 1).OrderByDescending(o=>o.DateCreated).ToList();
            return PostList;
        }

        // GET api/posts/single/
        [HttpGet("post/{postId}")] //get specific post
        [AllowAnonymous]
        public ActionResult<Post> GetPost(long postId)
        {
            Post post = _context.Posts.First(o=>o.Id == postId);
            return post;
        }

        [HttpGet("drafts")]
        public ActionResult<IEnumerable<Post>> GetDrafts(long postId)
        {
            List<Post> PostList = _context.Posts.Where(o=>o.IsDraft == 1 && o.UserId == GetUserId()).OrderByDescending(o=>o.Id).ToList();
            return PostList;
        }

        [HttpGet("draft/{draftId}")]
        public ActionResult<Post> GetDraft(long draftId)
        {
            Post post = _context.Posts.First(o=>o.Id == draftId);
            if(GetUserId() != post.UserId){
                return StatusCode(401);
            }
            return post;
        }

        
        [HttpGet("feed")]
        public ActionResult<IEnumerable<Post>> Get()
        {
            User user = _context.Users.First(o=>o.Id == GetUserId());
            
            string followingString = user.Followings;
            if(followingString == null || followingString == ""){
                return NotFound();
            }

            //10,000 ticks per milisecond
            //long month = 25920000000000; //ticks in a month
            long week = 6048000000000; //ticks in a week
            DateTime pastTime =  new DateTime(DateTime.UtcNow.Ticks - week*2);
            List<Post> PostList = _context.Posts.Where(o=>o.DateCreated > pastTime && o.IsDraft != 1).OrderByDescending(o=>o.DateCreated).ToList();
            List<Post> Feed = new List<Post>();
            String[] usernames = followingString.Split(";");
            var userIds = new List<long>();

            foreach(string username in usernames){
                userIds.Add(_context.Users.First(o=>o.UserName == username).Id);
            }
            /*long[] userIds = Array.ConvertAll(followingString.Split(";"), long.Parse);
            foreach(Post post in PostList){
                foreach(long id in userIds){
                    if(post.UserId == id){
                        Feed.Add(post);
                        break;
                    }
                }
                if(Feed.Count<=20){
                    break;
                }
            }*/

            foreach(Post post in PostList){
                foreach(long id in userIds){
                    if(post.UserId == id){
                        Feed.Add(post);
                        break;
                    }
                }
                if(Feed.Count<=20){
                    break;
                }
            }

            return Feed;
        }

        [HttpPost]
        public async Task<Object> POST([FromBody]Post post){

            long userId = GetUserId();

            if(post.IsDraft == 1){
                post.DateCreated = null;
            }else{
                User user = _context.Users.First(o=>o.Id == userId);
                user.TotalPosts += 1;
                _context.Entry(user).State = EntityState.Modified;
            }
            post.UserId = userId;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            //return Ok(new { response = "Record has been successfully added"});
            return Ok(new { response = "Record has been successfully added"});
        }

        [HttpPut]
        public async Task<Object> Edit([FromBody]Post post){

            Post currentPost = _context.Posts.First(o=>o.Id == post.Id);

            if(currentPost.UserId != GetUserId()){
                return StatusCode(401);
            }else{
                post.UserId = currentPost.UserId;
            }

            if(currentPost == null)
                return Ok(new { response = "Cannot update a post that hasn't been created"});

            if(post.IsDraft == 0){

                //Post currentPost = _context.Posts.First(o=>o.Id == post.Id);
                //if the draft state changes add 1 to total posts for the user that posted it
                if (currentPost.IsDraft == 1){
                    User user = _context.Users.First(o=>o.Id == post.UserId);
                    user.TotalPosts += 1;
                    _context.Entry(user).State = EntityState.Modified;
                    post.DateCreated = DateTime.UtcNow;
                }
                if(post.DateCreated != null){
                    post.DateModified = DateTime.UtcNow;
                }
            }
            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { response = "Post has been successfully updated"});
        }

        [HttpPost("delete")]
        public async Task<Object> Delete([FromBody]Post post){

            if(post.IsDraft != 1){
                User user = _context.Users.First(o=>o.Id == post.UserId);
                user.TotalPosts -= 1;
                _context.Entry(user).State = EntityState.Modified;
            }

            _context.Entry(post).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Ok(new { response = "Post has been successfully deleted"});
        }
    }
}