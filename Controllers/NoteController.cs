using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Challenge.Dtos.Note;
using Challenge.Models;
using Challenge.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Challenge.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NoteController> _logger;
        public NoteController(INoteRepository noteRepository, IMapper mapper, ILogger<NoteController> logger)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(int userId)
        {
            try
            {
                var notes = await _noteRepository.GetUserNotes(userId);
                var noteDto = _mapper.Map<IEnumerable<NoteDto>>(notes);
                return Ok(noteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error Performing GET in {nameof(GetAll)}");
                return StatusCode(500);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var note = await _noteRepository.GetAsync(id);
                if (note == null)
                {
                    return NotFound();
                }
                var noteDto = _mapper.Map<NoteDto>(note);
                return Ok(noteDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error Performing GET in {nameof(Get)}");
                return StatusCode(500);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NoteCreateDto noteDto)
        {
            try
            {
                var note = _mapper.Map<Note>(noteDto);
                await _noteRepository.AddAsync(note);
                await _noteRepository.SaveAsync();
                return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error Performing POST in {nameof(Create)}", noteDto);
                return StatusCode(500);
            }
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] NoteUpdateDto noteDto)
        {
            var note = await _noteRepository.GetAsync(noteDto.Id);
            if (note == null)
            {
                return NotFound();
            }
            _mapper.Map(noteDto, note);
            _noteRepository.Update(note);
            try
            {
                await _noteRepository.SaveAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex.Message, $"Error Performing GET in {nameof(Update)}");
                return StatusCode(500);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var note = await _noteRepository.GetAsync(id);
                if (note == null)
                {
                    return NotFound();
                }
                _noteRepository.Remove(note);
                await _noteRepository.SaveAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error Performing DELETE in {nameof(Delete)}");
                return StatusCode(500);
            }
        }
    }
}