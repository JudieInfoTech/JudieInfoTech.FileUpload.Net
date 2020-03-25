using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JudieInfoTech.FileUpload.Net.Resources.Service
{
  public class ResourceResult<Resource, TKey> where TKey : IEquatable<TKey>
  {
    public IList<ValidationResult> Errors { get; private set; }

    public Exception Exception { get; set; }

    public Resource Object { get; set; }
  }
}
