using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engli3m.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SubmitAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EQuizSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQuizId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQuizSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EQuizSubmissions_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EQuizSubmissions_EQuizzes_EQuizId",
                        column: x => x.EQuizId,
                        principalTable: "EQuizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EQuestionSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EQuizSubmissionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedAnswerId = table.Column<int>(type: "int", nullable: true),
                    WrittenAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EarnedPoints = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EQuestionSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EQuestionSubmissions_EQuizSubmissions_EQuizSubmissionId",
                        column: x => x.EQuizSubmissionId,
                        principalTable: "EQuizSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EQuestionSubmissions_QuestionsAnswers_SelectedAnswerId",
                        column: x => x.SelectedAnswerId,
                        principalTable: "QuestionsAnswers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EQuestionSubmissions_QuizQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EQuestionSubmissions_EQuizSubmissionId",
                table: "EQuestionSubmissions",
                column: "EQuizSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_EQuestionSubmissions_QuestionId",
                table: "EQuestionSubmissions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EQuestionSubmissions_SelectedAnswerId",
                table: "EQuestionSubmissions",
                column: "SelectedAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_EQuizSubmissions_EQuizId",
                table: "EQuizSubmissions",
                column: "EQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_EQuizSubmissions_StudentId",
                table: "EQuizSubmissions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EQuestionSubmissions");

            migrationBuilder.DropTable(
                name: "EQuizSubmissions");
        }
    }
}
