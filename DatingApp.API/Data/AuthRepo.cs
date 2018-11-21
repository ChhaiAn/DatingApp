using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    //Querying DB via Entity Framework
    public class AuthRepo : IAuthRepo
    {
        //Inject DataContext
        private readonly DataContext _context;
        public AuthRepo(DataContext context)
        {
            _context = context;

        }
        public  async Task<User> Login(string username, string password)
        {
            // get username if it match from user input
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            
            if(user == null)
            {
                return null;
            }

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) 
            {
                return null;
            }
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using ( var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) 
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                //each char in computedHash must match what saved in db(password)
                for(int i=0; i< computedHash.Length;i++) 
                {
                    if(computedHash[i] != passwordHash[i]) {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            
            //"out" pass as reference instead of value
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            return user;

        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // "using" make sure that after this method everything will be disposed
            // HMACSHAR512 return key can be use as salt
            using ( var hmac = new System.Security.Cryptography.HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                // ComputeHash need byte type so we convert our sting "password" to byte (GetBytes)
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
          
           
        }

        public  async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(x => x.Username == username)) {
                return true;
            }
            return false;
        }
    }
}