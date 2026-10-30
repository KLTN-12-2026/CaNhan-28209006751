using Atomos.Domain.Admin;
using Atomos.Domain.Users;
using Atomos.Domain.Challenges;
using Atomos.Domain.Chemistry;
using Atomos.Domain.Learning;
using Atomos.Domain.About;
using Atomos.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Application.Common;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<AuthSession> AuthSessions { get; }
    DbSet<Chemical> Chemicals { get; }
    DbSet<Reaction> Reactions { get; }
    DbSet<ReactionParticipant> ReactionParticipants { get; }
    DbSet<ReactionRule> ReactionRules { get; }
    DbSet<ReactionRuleSlot> ReactionRuleSlots { get; }
    DbSet<ReactionRuleCondition> ReactionRuleConditions { get; }
    DbSet<ReactionRuleProduct> ReactionRuleProducts { get; }
    DbSet<ReactionRuleOutcome> ReactionRuleOutcomes { get; }
    DbSet<PeriodicElement> PeriodicElements { get; }
    DbSet<ChemicalComponent> ChemicalComponents { get; }
    DbSet<ElementOxidationState> ElementOxidationStates { get; }
    DbSet<ChemicalOxidationState> ChemicalOxidationStates { get; }
    DbSet<ChemicalRedoxCapability> ChemicalRedoxCapabilities { get; }
    DbSet<ChemicalWaterReactivity> ChemicalWaterReactivities { get; }
    DbSet<ChemicalAmphotericBehavior> ChemicalAmphotericBehaviors { get; }
    DbSet<ChemicalAcidBehavior> ChemicalAcidBehaviors { get; }
    DbSet<ChemicalProductFamily> ChemicalProductFamilies { get; }
    DbSet<UserChemical> UserChemicals { get; }
    DbSet<Chapter> Chapters { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<LearningProfile> LearningProfiles { get; }
    DbSet<LearningNote> LearningNotes { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseSet> ExerciseSets { get; }
    DbSet<ExerciseSetQuestion> ExerciseSetQuestions { get; }
    DbSet<ExerciseChoice> ExerciseChoices { get; }
    DbSet<Exam> Exams { get; }
    DbSet<ExamExercise> ExamExercises { get; }
    DbSet<LearningAttempt> LearningAttempts { get; }
    DbSet<LearningAttemptDetail> LearningAttemptDetails { get; }
    DbSet<AboutContent> AboutContents { get; }
    DbSet<UsageQuota> UsageQuotas { get; }
    DbSet<AdminAuditLog> AdminAuditLogs { get; }
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<KnowledgeProposal> KnowledgeProposals { get; }
    DbSet<KnowledgeProposalSource> KnowledgeProposalSources { get; }
    DbSet<KnowledgeProposalRevision> KnowledgeProposalRevisions { get; }
    DbSet<KnowledgeProposalJob> KnowledgeProposalJobs { get; }
    DbSet<PaymentConfiguration> PaymentConfigurations { get; }
    DbSet<PaymentOrder> PaymentOrders { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<WorkshopChallenge> WorkshopChallenges { get; }
    DbSet<WorkshopChallengeResource> WorkshopChallengeResources { get; }
    DbSet<WorkshopChallengeSession> WorkshopChallengeSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
