using Microsoft.EntityFrameworkCore;
using Agg = Rent2Go.API.IAM.Domain.Model.Aggregates;
using DMain = Rent2Go.API.IAM.Domain.Model;
using BackendRent2Go.Client.Domain.Model.Entities;

namespace BackendRent2Go.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Agg.User> Users { get; set; }
        public DbSet<DMain.Client> Clients { get; set; }
        public DbSet<DMain.Provider> Providers { get; set; }
        public DbSet<DMain.EmailVerification> EmailVerifications { get; set; }
        public DbSet<Rent2Go.API.Provider.Domain.Model.Vehicle> Vehicles { get; set; }
        public DbSet<Rent2Go.API.Provider.Domain.Model.VehicleSpecification> VehicleSpecifications { get; set; }
        public DbSet<Rent2Go.API.Provider.Domain.Model.VehicleImage> VehicleImages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignorar la clase User duplicada para evitar conflictos de mapeo
            modelBuilder.Ignore<Rent2Go.API.IAM.Domain.Model.User>();

            modelBuilder.Entity<Agg.User>().ToTable("users");
            modelBuilder.Entity<DMain.Client>().ToTable("clients");
            modelBuilder.Entity<DMain.Provider>().ToTable("providers");
            modelBuilder.Entity<DMain.EmailVerification>().ToTable("email_verifications");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().ToTable("vehicles");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().ToTable("vehicle_specifications");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().ToTable("vehicle_images");
            modelBuilder.Entity<Reservation>().ToTable("reservations");
            modelBuilder.Entity<PaymentMethod>().ToTable("payment_methods");
            modelBuilder.Entity<Payment>().ToTable("payments");

            modelBuilder.Entity<Agg.User>().Property(u => u.Id).HasColumnName("id");
            modelBuilder.Entity<Agg.User>().Property(u => u.Name).HasColumnName("name");
            modelBuilder.Entity<Agg.User>().Property(u => u.Email).HasColumnName("email");
            modelBuilder.Entity<Agg.User>().Property(u => u.Password).HasColumnName("password");
            modelBuilder.Entity<Agg.User>().Property(u => u.Role).HasColumnName("role");
            modelBuilder.Entity<Agg.User>().Property(u => u.Status).HasColumnName("status");
            modelBuilder.Entity<Agg.User>().Property(u => u.ProfileImage).HasColumnName("profile_image");
            modelBuilder.Entity<Agg.User>().Property(u => u.OAuthProvider).HasColumnName("oauth_provider");
            modelBuilder.Entity<Agg.User>().Property(u => u.OAuthId).HasColumnName("oauth_id");
            modelBuilder.Entity<Agg.User>().Property(u => u.IsOAuthUser).HasColumnName("is_oauth_user");
            modelBuilder.Entity<Agg.User>().Property(u => u.EmailVerified).HasColumnName("email_verified");
            modelBuilder.Entity<Agg.User>().Property(u => u.RegistrationDate).HasColumnName("registration_date");

            modelBuilder.Entity<DMain.Client>().Property(c => c.Id).HasColumnName("id");
            modelBuilder.Entity<DMain.Client>().Property(c => c.UserId).HasColumnName("user_id");
            modelBuilder.Entity<DMain.Client>().Property(c => c.Dni).HasColumnName("dni");
            modelBuilder.Entity<DMain.Client>().Property(c => c.Phone).HasColumnName("phone");
            modelBuilder.Entity<DMain.Client>().Property(c => c.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<DMain.Client>().Property(c => c.UpdatedAt).HasColumnName("updated_at");

            modelBuilder.Entity<DMain.Provider>().Property(p => p.Id).HasColumnName("id");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.UserId).HasColumnName("user_id");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.BusinessName).HasColumnName("business_name");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.TaxId).HasColumnName("tax_id");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.Phone).HasColumnName("phone");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.Status).HasColumnName("status");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<DMain.Provider>().Property(p => p.UpdatedAt).HasColumnName("updated_at");

            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.Id).HasColumnName("id");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.UserId).HasColumnName("user_id");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.Email).HasColumnName("email");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.VerificationToken).HasColumnName("verification_token");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.ExpiresAt).HasColumnName("expires_at");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<DMain.EmailVerification>().Property(e => e.VerifiedAt).HasColumnName("verified_at");

            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Id).HasColumnName("id");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.OwnerId).HasColumnName("provider_id");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Brand).HasColumnName("brand");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Model).HasColumnName("model");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Year).HasColumnName("year");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Type).HasColumnName("type");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.DailyRate).HasColumnName("daily_rate");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Description).HasColumnName("description");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Location).HasColumnName("location");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.Status).HasColumnName("status");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.Vehicle>().Property(v => v.UpdatedAt).HasColumnName("updated_at");

            // Mapeo de VehicleSpecification
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.VehicleId).HasColumnName("vehicle_id");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.Doors).HasColumnName("doors");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.Seats).HasColumnName("seats");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.Transmission).HasColumnName("transmission");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.FuelType).HasColumnName("fuel_type");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleSpecification>().Property(vs => vs.AirConditioner).HasColumnName("air_conditioner");
            
            // Mapeo de VehicleImage
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().Property(vi => vi.Id).HasColumnName("id");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().Property(vi => vi.VehicleId).HasColumnName("vehicle_id");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().Property(vi => vi.ImageUrl).HasColumnName("image_url");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().Property(vi => vi.IsPrimary).HasColumnName("is_primary");
            modelBuilder.Entity<Rent2Go.API.Provider.Domain.Model.VehicleImage>().Property(vi => vi.CreatedAt).HasColumnName("created_at");

            // Configuración para Reservation
            modelBuilder.Entity<Reservation>().Property(r => r.Id).HasColumnName("id");
            modelBuilder.Entity<Reservation>().Property(r => r.ClientId).HasColumnName("client_id");
            modelBuilder.Entity<Reservation>().Property(r => r.ProviderId).HasColumnName("provider_id");
            modelBuilder.Entity<Reservation>().Property(r => r.VehicleId).HasColumnName("vehicle_id");
            modelBuilder.Entity<Reservation>().Property(r => r.StartDate).HasColumnName("start_date");
            modelBuilder.Entity<Reservation>().Property(r => r.EndDate).HasColumnName("end_date");
            modelBuilder.Entity<Reservation>().Property(r => r.Status).HasColumnName("status");
            modelBuilder.Entity<Reservation>().Property(r => r.PaymentStatus).HasColumnName("payment_status");
            modelBuilder.Entity<Reservation>().Property(r => r.PaymentMethod).HasColumnName("payment_method");
            modelBuilder.Entity<Reservation>().Property(r => r.TotalAmount).HasColumnName("total_amount");
            modelBuilder.Entity<Reservation>().Property(r => r.VehiclePrice).HasColumnName("vehicle_price");
            modelBuilder.Entity<Reservation>().Property(r => r.Location).HasColumnName("location");
            modelBuilder.Entity<Reservation>().Property(r => r.Notes).HasColumnName("notes");
            modelBuilder.Entity<Reservation>().Property(r => r.CancellationReason).HasColumnName("cancellation_reason");
            modelBuilder.Entity<Reservation>().Property(r => r.CancellationDate).HasColumnName("cancellation_date");
            modelBuilder.Entity<Reservation>().Property(r => r.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Reservation>().Property(r => r.UpdatedAt).HasColumnName("updated_at");

            // Configuración para PaymentMethod
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.Id).HasColumnName("id");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.UserId).HasColumnName("user_id");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.Type).HasColumnName("type");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.IsDefault).HasColumnName("is_default");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.CardHolder).HasColumnName("card_holder");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.CardNumberLast4).HasColumnName("card_number_last4");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.CardExpiry).HasColumnName("card_expiry");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.CardType).HasColumnName("card_type");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.PaypalEmail).HasColumnName("paypal_email");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.BankName).HasColumnName("bank_name");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.AccountNumberLast4).HasColumnName("account_number_last4");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<PaymentMethod>().Property(pm => pm.UpdatedAt).HasColumnName("updated_at");

            // Configuración para Payment
            modelBuilder.Entity<Payment>().Property(p => p.Id).HasColumnName("id");
            modelBuilder.Entity<Payment>().Property(p => p.ReservationId).HasColumnName("reservation_id");
            modelBuilder.Entity<Payment>().Property(p => p.PaymentMethodId).HasColumnName("payment_method_id");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnName("amount");
            modelBuilder.Entity<Payment>().Property(p => p.Status).HasColumnName("status");
            modelBuilder.Entity<Payment>().Property(p => p.TransactionId).HasColumnName("transaction_id");
            modelBuilder.Entity<Payment>().Property(p => p.PaymentDate).HasColumnName("payment_date");
            modelBuilder.Entity<Payment>().Property(p => p.Notes).HasColumnName("notes");
            modelBuilder.Entity<Payment>().Property(p => p.CreatedAt).HasColumnName("created_at");
            modelBuilder.Entity<Payment>().Property(p => p.UpdatedAt).HasColumnName("updated_at");

            // Relaciones
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany()
                .HasForeignKey(p => p.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany()
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
