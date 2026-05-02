using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConversationObjectInMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages");

            migrationBuilder.AddColumn<Guid>(
                name: "ConversationEntityId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationEntityId",
                table: "Messages",
                column: "ConversationEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationEntityId",
                table: "Messages",
                column: "ConversationEntityId",
                principalTable: "Conversations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationEntityId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ConversationEntityId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ConversationEntityId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
