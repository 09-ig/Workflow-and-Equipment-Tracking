using System;
using AeroMaintain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AeroMaintain.Migrations;

[DbContext(typeof(AeroMaintainDbContext))]
partial class AeroMaintainDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

        modelBuilder.Entity("AeroMaintain.Models.AuditLog", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("TEXT");

            b.Property<string>("Action")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<DateTime>("ChangedAtUtc")
                .HasColumnType("TEXT");

            b.Property<string>("Description")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<Guid?>("EntityId")
                .HasColumnType("TEXT");

            b.Property<string>("EntityName")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("FieldName")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("NewValue")
                .HasColumnType("TEXT");

            b.Property<string>("OldValue")
                .HasColumnType("TEXT");

            b.Property<string>("UserName")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("UserRole")
                .IsRequired()
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("ChangedAtUtc");

            b.HasIndex("EntityName", "EntityId");

            b.ToTable("AuditLogs");
        });

        modelBuilder.Entity("AeroMaintain.Models.Equipment", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("TEXT");

            b.Property<string>("Category")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<DateTime>("LastMaintenanceDate")
                .HasColumnType("TEXT");

            b.Property<int>("MaintenanceIntervalDays")
                .HasColumnType("INTEGER");

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Notes")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<int>("RecentIssueCount")
                .HasColumnType("INTEGER");

            b.Property<string>("SerialNumber")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Status")
                .IsRequired()
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("Equipment");
        });

        modelBuilder.Entity("AeroMaintain.Models.MaintenanceLog", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("TEXT");

            b.Property<DateTime>("CompletedOn")
                .HasColumnType("TEXT");

            b.Property<decimal>("Cost")
                .HasColumnType("TEXT");

            b.Property<Guid>("EquipmentId")
                .HasColumnType("TEXT");

            b.Property<string>("EquipmentName")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<double>("LaborHours")
                .HasColumnType("REAL");

            b.Property<DateTime>("LoggedAtUtc")
                .HasColumnType("TEXT");

            b.Property<string>("Notes")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("PartsReplaced")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("PerformedBy")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("WorkSummary")
                .IsRequired()
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("EquipmentId", "CompletedOn");

            b.ToTable("MaintenanceLogs");
        });

        modelBuilder.Entity("AeroMaintain.Models.MaintenanceAssignment", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("TEXT");

            b.Property<string>("ApprovedBy")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<DateTime?>("ApprovedAtUtc")
                .HasColumnType("TEXT");

            b.Property<string>("AssignedBy")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("AssignedTo")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<Guid?>("CompletionMaintenanceLogId")
                .HasColumnType("TEXT");

            b.Property<DateTime?>("CompletedAtUtc")
                .HasColumnType("TEXT");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("TEXT");

            b.Property<DateTime>("DueDate")
                .HasColumnType("TEXT");

            b.Property<Guid>("EquipmentId")
                .HasColumnType("TEXT");

            b.Property<string>("EquipmentName")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Notes")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Priority")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("SerialNumber")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Status")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("WorkSummary")
                .IsRequired()
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex("EquipmentId", "Status", "DueDate");

            b.ToTable("MaintenanceAssignments");
        });

        modelBuilder.Entity("AeroMaintain.Models.TroubleshootingRule", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("TEXT");

            b.Property<string>("PossibleCauses")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("RecommendedChecks")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Symptom")
                .IsRequired()
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("TroubleshootingRules");
        });

        modelBuilder.Entity("AeroMaintain.Models.MaintenanceLog", b =>
        {
            b.HasOne("AeroMaintain.Models.Equipment", "Equipment")
                .WithMany()
                .HasForeignKey("EquipmentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Equipment");
        });

        modelBuilder.Entity("AeroMaintain.Models.MaintenanceAssignment", b =>
        {
            b.HasOne("AeroMaintain.Models.Equipment", "Equipment")
                .WithMany()
                .HasForeignKey("EquipmentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Equipment");
        });
    }
}
